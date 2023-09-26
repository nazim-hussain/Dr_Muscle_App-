using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Controls;
using DrMuscle.Dependencies;
using DrMuscle.Enums;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Models;
using DrMuscle.Resx;
using DrMuscle.Screens.Subscription;
using DrMuscle.Screens.User;
using DrMuscle.Views;
using DrMuscleWebApiSharedModel;
using Microcharts;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;


namespace DrMuscle.Screens.Eve
{
    public partial class MealInfoPage : DrMusclePage
    {
        public ObservableCollection<BotModel> BotList = new ObservableCollection<BotModel>();
        private bool IsBodyweightPopup = false;
        decimal _hour;
        //IMealPlanService _mealPlanServices;
        //IMealServices _mealServices;
        //IUserService _userService;

        string LblWeightGoal = "";

        IFirebase _firebase;

        CustomImageButton BtnKeto;
        CustomImageButton BtnPaleo;
        CustomImageButton BtnVegan;
        CustomImageButton BtnMediterranean;
        CustomImageButton BtnVegetarian;

        CustomImageButton BtnNoPreference;

        CustomImageButton Btn12Times;
        CustomImageButton Btn34Times;
        CustomImageButton Btn5PlusTimes;
        CustomImageButton BtnIdontExercise;

        CustomImageButton BtnIMostlySit;
        CustomImageButton BtnIMostlyStandWalk;
        CustomImageButton BtnIdoManualOrPhysicalWork;

        CustomImageButton BtnFish;
        CustomImageButton BtnEgg;
        CustomImageButton BtnDairyProduct;
        CustomImageButton BtnRedMeat;
        CustomImageButton BtnPoultry;
        CustomImageButton BtnNuts;
        CustomImageButton BtnBeans;
        CustomImageButton BtnTofu;

        Grid btnLog;
        Button btnBodyWeight;
        Button btnGetPlan;
        bool IsFish = false;
        bool IsEggs = false;
        bool IsDairy = false;
        bool IsRedMeat = false;
        bool IsPoultry = false;
        bool IsNuts = false;
        bool IsBeans = false;
        bool IsTofu = false;
        bool IsSimpleMealPlan = false;
        public string VegetarianEats = "";

        public bool IsAnyAllergies = false;
        public string AllergyText = "";

        public string FavouriteFood = "";
        public string ExericseTime;
        public string ActiveOnJob;

        public MealInfoPage()
        {
            InitializeComponent();
            lstChats.ItemsSource = BotList;
            Title = "Meal plan";

            MessagingCenter.Subscribe<SubscriptionModel>(this, "SubscriptionPurchaseMessage", (obj) =>
            {
                if (obj == null)
                    return;
                UpdateSubscriptionData(obj);
            });

            MessagingCenter.Subscribe<SubscriptionModel>(this, "SubscriptionPurchaseIfNotExistMessage", (obj) =>
            {
                if (obj == null)
                    return;
                AddSubscriptionDataIfNotExist(obj);
            });
            _firebase = DependencyService.Get<IFirebase>();
            //DependencyService.Get<IDrMuscleSubcription>().OnMealPlanAccessPurchased += async delegate {
            //    App.IsMealPlan = true;
            //    if (Device.RuntimePlatform.Equals(Device.Android))
            //     UserDialogs.Instance.AlertAsync(new AlertConfig() { AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray), Message= "Your purchase was successful.", Title="You're all set", OkText = "OK" });
            //    BotList.Clear();
            //};
        }

        public override async void OnBeforeShow()
        {
            base.OnBeforeShow();
            BotList.Clear();
            stackOptions.Children.Clear();
            try
            {
                FabImage.IsVisible = false;
                if (!App.IsMealPlan)
                {
                    mainScroll.IsVisible = false;
                    //MainGrid.IsVisible = false;
                    //mainScroll.IsVisible = true;
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                var modalPage = new Views.GeneralPopup("Lists.png", "New! Meal plans—limited-time discount", "Get in shape faster with meal plans that update on autopilot. Discounted for a limited time to celebrate.", "Got it",null,false,false,"false","false","false");
                modalPage.Disappearing += (sender2, e2) =>
                {
                    waitHandle.Set();
                };
                    //modalPage.OkButtonPress += ModalPage_OkButtonPress;
                await PopupNavigation.Instance.PushAsync(modalPage);

                await Task.Run(() => waitHandle.WaitOne());

                    //return;
                }
                
                CheckHeight();

            }
            catch (Exception ex)
            {

            }
        }


        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (BotList.Count == 0)
                StartSetup();

            try
            {
                var result = "";
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
                var result1 = "";
                if (lowReps >= 5 && highreps <= 12)
                {
                    result = "Build muscle and strength";
                    result1 = "building muscle and strength";
                }
                else if (lowReps >= 8 && highreps <= 15)
                {
                    result = "Build muscle and burn fat";
                    result1 = "building muscle and burn fat";
                }
                else if (lowReps >= 5 && highreps <= 15)
                {
                    result = "Build muscle";
                    result1 = "building muscle";
                }
                else if (lowReps >= 12 && highreps <= 20)
                {
                    result = "Burn fat";
                    result1 = "burning fat";
                }
                else if (highreps >= 16)
                {
                    result = "Build muscle and burn fat";
                    result1 = "building muscle and burning fat";
                }
                else
                {
                    if (LocalDBManager.Instance.GetDBSetting("Demoreprange") != null)
                    {
                        if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                        {
                            result = "Build muscle";
                            result1 = "building muscle";
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                        {
                            result = "Build muscle and burn fat";
                            result1 = "building muscle and burning fat";
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                        {
                            result = "Burn fat";
                            result1 = "burning fat";
                        }
                    }
                }
                
                LblWeightGoal = $"Track your weight to get custom tip to {result.ToLower()}.";


                if (App.IsOnboarding)
                {
                    var text = "";
                    try
                    {
                        if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscle")
                        {
                            text = "Build muscle";
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "BuildMuscleBurnFat")
                        {
                            text = "Build muscle and burn fat";
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                        {
                            text = "Burn fat";
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                   
                 
                    App.IsOnboarding = false;
                    //XX workouts / week
                    var workoutname = "";
                    var count = 3;
                    int age = -1, xDays = 3;
                    if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                    {

                        workoutname = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value;

                        if (LocalDBManager.Instance.GetDBSetting("Age") != null && LocalDBManager.Instance.GetDBSetting("Age").Value != null)
                        {
                            age = int.Parse(LocalDBManager.Instance.GetDBSetting("Age").Value);
                        }
                        if (age != -1)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("push/pull/legs"))
                            {
                                xDays = 6;
                            }
                            else if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("split"))
                            {
                                if (age < 30)
                                    xDays = 5;
                                else if (age >= 30 && age <= 50)
                                    xDays = 4;
                                else
                                    xDays = 3;
                            }
                            else
                            {
                                if (age < 30)
                                    xDays = 4;
                                else if (age >= 30 && age <= 50)
                                    xDays = 3;
                                else
                                    xDays = 2;
                            }
                        }
                    }
                   
                    MessagingCenter.Send<BodyweightUpdateMessage>(new BodyweightUpdateMessage(), "BodyweightUpdateMessage");
                    //return;

                }
            }
            catch (Exception ex)
            {

            }
            LoadSavedWeights();
            if (!Config.IsMealEntered && !IsBodyweightPopup)
            {
                if (!Config.IsMeal1)
                {
                    var meal1 = new MealGeneralPopup();
                    meal1.SetPopupTitle("Yesterday, what did you have for breakfast?", GeneralPopupEnum.Meal1, "", "List all foods");
                    IsBodyweightPopup = true;
                    PopupNavigation.Instance.PushAsync(meal1);
                }
                else if (!Config.IsMeal2)
                {
                    var meal2 = new MealGeneralPopup();
                    meal2.SetPopupTitle("Yesterday, what did you have for lunch?", GeneralPopupEnum.Meal2, "", "List all foods you remember");
                    IsBodyweightPopup = true;
                    PopupNavigation.Instance.PushAsync(meal2);
                }
                else if (!Config.IsMeal3)
                {
                    var meal3 = new MealGeneralPopup();
                    meal3.SetPopupTitle("Yesterday, what did you have for diner?", GeneralPopupEnum.Meal3, "", "List all foods you remember");
                    IsBodyweightPopup = true;
                    PopupNavigation.Instance.PushAsync(meal3);
                }

            }
            else if (App.IsDisplayPopup)
            {
                App.IsDisplayPopup = false;
                var mealInfo = new MealInfoPopup();
                PopupNavigation.Instance.PushAsync(mealInfo);
            }
            else
            {
                //var dt = new DateTime(Config.LastBodyWeightUpdate);
                //if ((DateTime.Now.Date - dt).TotalDays >= 7)
                //{
                //    //Update bodyweight here:
                //    var bodyWeight = new MealBodyweightPopup();
                //    bodyWeight.SetBodyWeightProperty(WeightType.UpdateBodyWeight);
                //    PopupNavigation.Instance.PushAsync(bodyWeight);
                //}
            }

            MessagingCenter.Subscribe<Message.AddedMealInfoMessage>(this, "AddedMealInfoMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                //App.IsFromNotification = false;
               
                SaveMealInfo(obj);
            });
            MessagingCenter.Subscribe<Message.BodyweightMessage>(this, "BodyweightMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                BodyWeightMassUnitMessage(obj);
            });
            MessagingCenter.Subscribe<Message.GeneralMessage>(this, "GeneralMessage", (obj) =>
            {
                IsBodyweightPopup = false;
                HandleGeneralMessage(obj);

            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingCenter.Unsubscribe<Message.AddedMealInfoMessage>(this, "AddedMealInfoMessage");
            MessagingCenter.Unsubscribe<Message.BodyweightMessage>(this, "BodyweightMessage");
            MessagingCenter.Unsubscribe<Message.GeneralMessage>(this, "GeneralMessage");
                 }

        protected override bool OnBackButtonPressed()
        {

            if (IsBodyweightPopup)
                return true;
            //Device.BeginInvokeOnMainThread(async () =>
            //{
            //    var result = await DisplayAlert("Exit", "Are you sure you want to exit", "Yes", "No");
            //    if (result)
            //    {
            //        var kill = DependencyService.Get<IKillAppService>();
            //        kill.ExitApp();
            //    }
            //});
            return false;

        }

        private async void CheckHeight()
        {
            MainGrid.IsVisible = true;
            mainScroll.IsVisible = false;
            if (new MultiUnityWeight(Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value, CultureInfo.InvariantCulture), "kg").Lb > 150 && LocalDBManager.Instance.GetDBSetting("Height")?.Value == null)
            {
                CurrentLog.Instance.IsHeightPopup = true;
                AddQuestion("How tall are you?");
                await Task.Delay(100);
                PopupNavigation.Instance.PushAsync(new UserHeightView());
                IsBodyweightPopup = true;
            }
            else
            {
                FavouriteDiet();
                FavouriteFood = "";
            }
        }
        private async void ModalPage_OkButtonPress(object sender, EventArgs e)
        {
            await Task.Delay(300);
            ConfirmConfig subscribeConfig = new ConfirmConfig()
            {

                Message = "Are you sure you want to subscribe to the meal plan add-on?",
                OkText = "Subscribe",
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                CancelText = AppResources.Cancel,
            };

            var x = await UserDialogs.Instance.ConfirmAsync(subscribeConfig);
            if (x)
                await DependencyService.Get<IDrMuscleSubcription>().BuyMealPlanAccess();
            //else
            //    PagesFactory.PopAsync();
        }
        
        private async void HandleGeneralMessage(GeneralMessage general)
        {
            if (general.PopupEnum == Enums.GeneralPopupEnum.UserHeight)
            {
                IsBodyweightPopup = false;
                if (!general.IsCanceled)
                {
                    await AddAnswer(general.GeneralText);
                    //Ask meal Plan question right away
                    ClearOptions();
                    FabImage.IsVisible = false;
                    FavouriteDiet();
                    FavouriteFood = "";
                }
                await Task.Delay(200);
            }

            if (general.PopupEnum == Enums.GeneralPopupEnum.Allergy)
            {
                AllergyText = general.GeneralText;
                if (!general.IsCanceled)
                { 
                await AddAnswer(general.GeneralText);
                
                }
                await Task.Delay(200);
                HowManyExercise();
            }
            if (general.PopupEnum == Enums.GeneralPopupEnum.Meal1)
            {

                await AddAnswer(general.GeneralText);
                var mealModel = new DmmMeal()
                {
                   // Id = Guid.NewGuid(),
                    MealInfo = $"Breakfast: {general.GeneralText}",
                    //UserId = Guid.Parse(Config.UserId)
                };
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("Please check your internet connection.", "Internet error");
                    HandleGeneralMessage(general);
                    return;
                }
                try
                {
                    UserDialogs.Instance.ShowLoading("Updating...");
                    var userMealModel = await DrMuscleRestClient.Instance.AddUserMealAsync(mealModel);
                    UserDialogs.Instance.HideLoading();
                    await Task.Delay(200);
                }
                catch
                {

                }
                Config.IsMeal1 = true;
                var meal2 = new MealGeneralPopup();
                meal2.SetPopupTitle("Yesterday, what did you have for lunch?", GeneralPopupEnum.Meal2, "", "List all foods you remember");
                IsBodyweightPopup = true;
                PopupNavigation.Instance.PushAsync(meal2);
            }
            if (general.PopupEnum == Enums.GeneralPopupEnum.Meal2)
            {

                await AddAnswer(general.GeneralText);
                var mealModel = new DmmMeal()
                {
                   // Id = Guid.NewGuid(),
                    MealInfo = $"Lunch: {general.GeneralText}",
                   // UserId = Guid.Parse(Config.UserId)
                };
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("Please check your internet connection.", "Internet error");
                    HandleGeneralMessage(general);
                    return;
                }
                try
                {
                    UserDialogs.Instance.ShowLoading("Updating...");
                    var userMealModel = await DrMuscleRestClient.Instance.AddUserMealAsync(mealModel);
                    UserDialogs.Instance.HideLoading();
                    await Task.Delay(200);
                }
                catch
                {

                }
                await Task.Delay(200);
                Config.IsMeal2 = true;
                var meal3 = new MealGeneralPopup();
                meal3.SetPopupTitle("Yesterday, what did you have for diner?", GeneralPopupEnum.Meal3, "", "List all foods you remember");
                IsBodyweightPopup = true;
                PopupNavigation.Instance.PushAsync(meal3);
            }
            if (general.PopupEnum == Enums.GeneralPopupEnum.Meal3)
            {

                await AddAnswer(general.GeneralText);
                var mealModel = new DmmMeal()
                {
                   // Id = Guid.NewGuid(),
                    MealInfo = $"Diner: {general.GeneralText}",
                   // UserId = Guid.Parse(Config.UserId)
                };
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("Please check your internet connection.", "Internet error");
                    HandleGeneralMessage(general);
                    return;
                }
                try
                {
                    UserDialogs.Instance.ShowLoading("Updating...");
                    var userMealModel = await DrMuscleRestClient.Instance.AddUserMealAsync(mealModel);
                    UserDialogs.Instance.HideLoading();
                    await Task.Delay(200);
                }
                catch
                {

                }
                await Task.Delay(200);
                Config.IsMeal3 = true;
                Config.IsMealEntered = true;
                BtnAddMealPref_Clicked(new Button(), EventArgs.Empty);
               
            }
        }
        
        private async void StartSetup()
        {
            try
            {

                if (BotList.Count == 0)
                {
                   // BotList.Add(new BotModel() { Type = BotType.Chart });
                                    }
                await Task.Delay(600);
                GetLastMealPlanDate();
                ClearOptions();
                if (btnGetPlan == null)
                {
                   // FabImage.IsVisible = true;
                    
                }
                else
                {
                    
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void GetLastMealPlanDate()
        {
            //if (Config.LastMealPlanOrderDate == null)
            //{
            //    var date = await _userService.GetLastMealPlanDateAsync(new UserModel() { Id = Guid.Parse(Config.UserId) });
            //    if (date != null)
            //        Config.LastMealPlanOrderDate = ((DateTime)date).ToString();
            //    var notification13 = new NotificationRequest
            //    {
            //        NotificationId = 110,
            //        Title = "No new meal plan?",
            //        Description = "Get new meal plan",
            //        ReturningData = "NewMealPlan",
            //        Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //        Schedule = { NotifyTime = DateTime.Now.AddSeconds((((DateTime)date).AddDays(15) - DateTime.Now).TotalSeconds) }
            //    };

            //    NotificationCenter.Current.Show(notification13);
            //}
            //else
            //{
            //    try
            //    {

            //        if ((DateTime.Parse(Config.LastMealPlanOrderDate)).Date < DateTime.Now.AddDays(-15))
            //        {
            //            var notification13 = new NotificationRequest
            //            {
            //                NotificationId = 110,
            //                Title = "No new meal plan?",
            //                Description = "Get new meal plan",
            //                ReturningData = "NewMealPlan",
            //                Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //                Schedule = { NotifyTime = DateTime.Now.AddSeconds((DateTime.Now.AddDays(15) - DateTime.Now).TotalSeconds) }
            //            };
            //        }
            //        else
            //        {
            //            var date = await _userService.GetLastMealPlanDateAsync(new UserModel() { Id = Guid.Parse(Config.UserId) });
            //            var notification13 = new NotificationRequest
            //            {
            //                NotificationId = 110,
            //                Title = "No new meal plan?",
            //                Description = "Get new meal plan",
            //                ReturningData = "NewMealPlan",
            //                Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //                Schedule = { NotifyTime = DateTime.Now.AddSeconds((((DateTime)date).AddDays(15) - DateTime.Now).TotalSeconds) }
            //            };
            //        }

            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}
        }
        private async void BtnUpdateBodyWeight_Clicked(object sender, EventArgs e)
        {
            ((Button)sender).Clicked -= BtnUpdateBodyWeight_Clicked;
            var bodyWeight = new MealBodyweightPopup();
            bodyWeight.SetBodyWeightProperty(WeightType.UpdateBodyWeight);
            //_firebase.LogEvent("update_bodyweight", "");
            await PopupNavigation.Instance.PushAsync(bodyWeight);
            ((Button)sender).Clicked += BtnUpdateBodyWeight_Clicked;
        }
        async void BtnAddMealPref_Clicked(object sender, EventArgs e)
        {
            ActionStack.IsVisible = FabImage.IsVisible = false;

            ((Button)sender).Clicked -= BtnAddMealPref_Clicked;
            //_firebase.LogEvent("get_a_new_meal_plan", "");
            ClearOptions();
            FavouriteDiet();
            FavouriteFood = "";

            ((Button)sender).Clicked += BtnAddMealPref_Clicked;
        }
        async void BtnAddMealLog_Clicked(object sender, EventArgs e)
        {
            ActionStack.IsVisible = FabImage.IsVisible = false;
            ((Button)sender).Clicked -= BtnAddMealLog_Clicked;
            //_firebase.LogEvent("log_a_meal", "");
            var mealInfo = new MealInfoPopup();
            PopupNavigation.Instance.PushAsync(mealInfo);
            ((Button)sender).Clicked += BtnAddMealLog_Clicked;
        }
        private async void BodyWeightMassUnitMessage(BodyweightMessage bodyweight)
        {
            ActionStack.IsVisible = FabImage.IsVisible = false;
            //if (App.IsFromNotification)
            //    return;
            //if (string.IsNullOrEmpty(Config.UserEmail))
            //    return;
            //else
            if (bodyweight.WeightType == WeightType.UpdateBodyWeight)
            {
                var unit = Config.MassUnit == "lb" ? "lbs" : "kg";
                await AddAnswer($"Body weight {bodyweight.BodyWeight} {unit}");
                await Task.Delay(300);
                Config.CurrentWeight = bodyweight.Weight.ToString();
                decimal goalWeight = 0;
                try
                {

                goalWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    Config.PredictedWeight = goalWeight.ToString();
                }
                catch (Exception ex)
                {

                }
                var weight = Config.MassUnit == "lb" ? Math.Round(new DrMuscleWebApiSharedModel.MultiUnityWeight(goalWeight, "kg").Lb, 2).ToString() : goalWeight.ToString();
                var targetUnit = Config.MassUnit == "lb" ? "lbs" : "kg";


                ConfirmConfig p = new ConfirmConfig()
                {

                    Message = "Is this still correct?",
                    Title = $"Target body weight: { weight} {targetUnit}",
                    OkText = "Yes, correct",
                    CancelText = "No, update"
                    

                };

                var res = await UserDialogs.Instance.ConfirmAsync(p);
                if (!res)
                {
                    var bodyWeight = new MealBodyweightPopup();
                    bodyWeight.SetBodyWeightProperty(WeightType.UpdatePredictedWeight);
                    PopupNavigation.Instance.PushAsync(bodyWeight);

                }
                else
                {

                    UserModel userModel = new UserModel()
                    {
                        CurrentWeight = Convert.ToDouble(Config.CurrentWeight.Replace(",", "."), CultureInfo.InvariantCulture),
                        PredictedWeight = Convert.ToDouble(Config.PredictedWeight.Replace(",", "."), CultureInfo.InvariantCulture),
                        //Id = Guid.Parse(Config.UserId)
                    };
                    UserDialogs.Instance.ShowLoading("Processing...");
                    //WeightChangedModel isUpdated = await _userService.UpdateBodyWeightAsync(userModel);
                    UserDialogs.Instance.HideLoading();
                    ClearOptions();
                    BotList.Clear();
                    //if (isUpdated == null)
                    //    StartSetup();
                    //else
                    //    CheckBodyweightStatus(isUpdated);
                    Config.LastBodyWeightUpdate = DateTime.Now.Date.Ticks;

                    UpdateBodyweightNotification();


                    
                }

            }
            else if (bodyweight.WeightType == WeightType.UpdatePredictedWeight)
            {
                var massunit = Convert.ToDouble(bodyweight.BodyWeight) > 1 ? Config.MassUnit == "lb" ? "lbs" : "kg" : Config.MassUnit;
                await AddAnswer($"Target body weight {bodyweight.BodyWeight} {massunit}");
                
                Config.PredictedWeight = bodyweight.Weight.ToString();

                
                UserModel userModel = new UserModel()
                {
                    CurrentWeight = Convert.ToDouble(Config.CurrentWeight.Replace(",", "."), CultureInfo.InvariantCulture),
                    PredictedWeight = Convert.ToDouble(Config.PredictedWeight.Replace(",", "."), CultureInfo.InvariantCulture),
                    //Id = Guid.Parse(Config.UserId)
                };
                UserDialogs.Instance.ShowLoading("Processing...");
                //WeightChangedModel isUpdated = await _userService.UpdateBodyWeightAsync(userModel);
                UserDialogs.Instance.HideLoading();



                ClearOptions();
                BotList.Clear();
                //if (isUpdated == null)
                //    StartSetup();
                //else
                //    CheckBodyweightStatus(isUpdated);
                UpdateBodyweightNotification();
                Config.LastBodyWeightUpdate = DateTime.Now.Date.Ticks;

                StartSetup();
                
            }
        }

        private async void CheckBodyweightStatus(WeightChangedModel change)
        {
            try
            {

                var CurrentWeight = Convert.ToDouble(Config.CurrentWeight.Replace(",", "."), CultureInfo.InvariantCulture);
                var PredictedWeight = Convert.ToDouble(Config.PredictedWeight.Replace(",", "."), CultureInfo.InvariantCulture);
                var changeUnit = Config.MassUnit == "lb" ? Math.Round(new DrMuscleWebApiSharedModel.MultiUnityWeight((decimal)change.ChangedUnit, "kg").Lb, 2).ToString() : Math.Round(change.ChangedUnit, 2).ToString();
                var massunit = Convert.ToDouble(changeUnit) > 1 ? Config.MassUnit == "lb" ? "lbs" : "kg" : Config.MassUnit;
                var popupTitle = "";
                if (change.ChangedPercentage > 0)
                    popupTitle = $"You have gained {changeUnit} {massunit} in {Math.Round(change.Days)} days";
                else if (change.ChangedPercentage == 0)
                    popupTitle = $"Your weight has not changed";
                else
                    popupTitle = $"You have lost {changeUnit} {massunit} in {Math.Round(change.Days)} days";
                bool isAccepted = false;

                var w = Config.MassUnit == "lb" ? Math.Round(new DrMuscleWebApiSharedModel.MultiUnityWeight((decimal)CurrentWeight, "kg").Lb) + " lbs" : (decimal)CurrentWeight + " kg";
                if (Config.LastMealPlanOrderDate != null)
                {
                    DateTime dt = Convert.ToDateTime(Config.LastMealPlanOrderDate);
                    if (dt.Date == DateTime.Now.Date)
                    {
                        await DisplayAlert(popupTitle, "Nice work! You should continue on the same plan.", "Got it");
                        return;
                    }
                }
                //Body loss
                if (CurrentWeight > PredictedWeight && change.ChangedPercentage > -0.1429)
                {
                    isAccepted = await DisplayAlert(popupTitle, "You should get a plan with fewer calories.", "Get plan", "Skip");
                }
                else if (CurrentWeight > PredictedWeight && change.ChangedPercentage < -0.4287)
                {
                    var isAcc = await DisplayAlert("Are you sure?", $"Seems you have lost a lot of weight. Are you sure {w} is correct?", $"{w} correct", "Cancel");
                    if (isAcc)
                    {
                        isAccepted = await DisplayAlert(popupTitle, "Weight updated. You should get a new plan.", "Get plan", "Skip");
                    }
                    else
                    {
                        StartSetup();
                        BtnUpdateBodyWeight_Clicked(new Button(), EventArgs.Empty);
                        return;
                    }
                    
                }
                else if (CurrentWeight > PredictedWeight && change.ChangedPercentage < -0.1429)
                {
                    await DisplayAlert(popupTitle, "Nice work! You should continue on the same plan.", "Got it");
                }



                //Body Gain

                else if (CurrentWeight <= PredictedWeight && change.ChangedPercentage < 0.0714)
                {
                    isAccepted = await DisplayAlert(popupTitle, "You should get a plan with more calories.", "Get plan", "Skip");
                }

                else if (CurrentWeight <= PredictedWeight && change.ChangedPercentage > 0.1429)
                {
                    var isAcc = await DisplayAlert("Are you sure?", $"Seems you have gain a lot of weight. Are you sure {w} is correct?", $"{w} correct", "Cancel");
                    if (isAcc)
                    {
                        isAccepted = await DisplayAlert(popupTitle, "Weight updated. You should get a new plan.", "Get plan", "Skip");
                    }
                    else
                    {
                        BotList.Clear();
                        StartSetup();
                        BtnUpdateBodyWeight_Clicked(new Button(), EventArgs.Empty);
                        return;
                    }
                    
                }
                else if (CurrentWeight <= PredictedWeight && change.ChangedPercentage > 0.0714)
                {
                    await DisplayAlert(popupTitle, "Nice work! You should continue on the same plan.", "Got it");
                }
                if (isAccepted)
                {
                    FavouriteFood = "";
                    FavouriteDiet();
                }
                else
                {
                    BotList.Clear();
                    StartSetup();
                }

            }
            catch (Exception ex)
            {

            }
        }

        public async void UpdateBodyweightNotification()
        {
            //NotificationCenter.Current.Cancel(105);
            //NotificationCenter.Current.Cancel(106);
            //NotificationCenter.Current.Cancel(107);
            //NotificationCenter.Current.Cancel(108);
            //var notification = new NotificationRequest
            //{
            //    NotificationId = 105,
            //    Title = "Eve Diet Coach",
            //    Description = "Time to update your body weight!",
            //    ReturningData = "Bodyweight",
            //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //    Schedule = {
            //    NotifyTime = DateTime.Now.AddSeconds(24 * 7 * 60 * 60)
            //    }
            //};
            //NotificationCenter.Current.Show(notification);
            //var notification8 = new NotificationRequest
            //{
            //    NotificationId = 106,
            //    Title = "Eve Diet Coach",
            //    Description = "Time to update your body weight!",
            //    ReturningData = "Bodyweight",
            //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //    Schedule = { NotifyTime = DateTime.Now.AddSeconds(24 * 8 * 60 * 60) }
            //};
            //NotificationCenter.Current.Show(notification8);
            //var notification10 = new NotificationRequest
            //{
            //    NotificationId = 107,
            //    Title = "Eve Diet Coach",
            //    Description = "Time to update your body weight!",
            //    ReturningData = "Bodyweight",
            //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //    Schedule = { NotifyTime = DateTime.Now.AddSeconds(24 * 10 * 60 * 60) }
            //};
            //NotificationCenter.Current.Show(notification10);

            //var notification13 = new NotificationRequest
            //{
            //    NotificationId = 108,
            //    Title = "Eve Diet Coach",
            //    Description = "Time to update your body weight!",
            //    ReturningData = "Bodyweight",
            //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
            //    Schedule = { NotifyTime = DateTime.Now.AddSeconds(24 * 13 * 60 * 60) }
            //};
            //NotificationCenter.Current.Show(notification13);
        }

        

        async void SaveMealInfo(AddedMealInfoMessage addedMealInfo)
        {
            string meal = addedMealInfo.MealInfoStr;

            if (addedMealInfo.IsCanceled)
            {
                BotList.Clear();
                StartSetup();
                return;
            }

            await AddAnswer(meal);
            await Task.Delay(300);
            await AddQuestion("Great!");
            await Task.Delay(300);
            
            var mealModel = new DmmMeal()
            {
                Id = Guid.NewGuid(),
                MealInfo = meal
                
                //UserId = Guid.Parse(Config.UserId)
            };
            mealModel.Id = Guid.NewGuid();
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync("Please check your internet connection", "Internet error");
                return;
            }
            try
            {
                UserDialogs.Instance.ShowLoading("Updating...");
                var userMealModel = await DrMuscleRestClient.Instance.AddUserMealAsync(mealModel);
                UserDialogs.Instance.HideLoading();
                if (userMealModel != null)
                {

                    if (userMealModel.MealCount == 3)
                    {
                        Config.IsMealEntered = true;
                        await UserDialogs.Instance.AlertAsync("Let's customize your meal plan", "Grats on saving 3 meals!", "Ok, customize");
                        FavouriteDiet();
                    }
                    else
                        AskForNextMeal();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();

                await UserDialogs.Instance.AlertAsync("Please check your internet connection and try again", "Internet error");

            }

        }


        private async void FavouriteDiet()
        {
            await AddQuestion("Favorite diet?");
            FavouriteFood = "";
            BtnKeto = await AddCheckbox("Keto", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "Keto";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnPaleo).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegan).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnMediterranean).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnNoPreference).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegetarian).Content).Children[0]).Source = "Undone.png";

            });

            BtnPaleo = await AddCheckbox("Paleo", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "Paleo";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnKeto).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegan).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnMediterranean).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnNoPreference).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegetarian).Content).Children[0]).Source = "Undone.png";

            });
            BtnVegan = await AddCheckbox("Vegan", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "Vegan";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnPaleo).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnKeto).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnMediterranean).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnNoPreference).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegetarian).Content).Children[0]).Source = "Undone.png";

            });
            BtnVegetarian = await AddCheckbox("Vegetarian", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "Vegetarian";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnPaleo).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnKeto).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegan).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnNoPreference).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnMediterranean).Content).Children[0]).Source = "Undone.png";
            });
            BtnMediterranean = await AddCheckbox("Mediterranean", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "Mediterranean";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnPaleo).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnKeto).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegan).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnNoPreference).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegetarian).Content).Children[0]).Source = "Undone.png";

            });

            BtnNoPreference = await AddCheckbox("No preference", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                FavouriteFood = "No preference";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnPaleo).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnKeto).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegan).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnMediterranean).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnVegetarian).Content).Children[0]).Source = "Undone.png";

            });

            await AddOptions("Continue", async (sender, ee) =>
            {
                if (string.IsNullOrEmpty(FavouriteFood))
                    return;
                await AddAnswer(FavouriteFood);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                
                ClearOptions();
                //if (FavouriteFood.Equals("Vegetarian"))
                //{
                    AskVegiterianOption();
                //}
                //else
                //{
                //    VegetarianEats = "";
                //    AnyAllergies();
                //}
            });
        }

        private async void AskVegiterianOption()
        {
            await AddQuestion("What sources of protein would you like to eat?");
            IsFish = false;
            IsEggs = false;
            IsDairy = false;
            IsRedMeat = false;
            IsPoultry = false;
            IsNuts = false;
            IsBeans = false;
            IsTofu = false;

            if (!FavouriteFood.Equals("Vegetarian"))
                BtnRedMeat = await AddCheckbox("Red meat", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsRedMeat = !IsRedMeat;
                img.Source = IsRedMeat ? "done.png" : "Undone.png";

            });
            if (!FavouriteFood.Equals("Vegetarian"))
                BtnPoultry = await AddCheckbox("Poultry", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsPoultry = !IsPoultry;
                img.Source = IsPoultry ? "done.png" : "Undone.png";

            });

            BtnFish = await AddCheckbox("Fish", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsFish = !IsFish;
                img.Source = IsFish ? "done.png" : "Undone.png";
                
            });

            BtnEgg = await AddCheckbox("Eggs", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsEggs = !IsEggs;
                img.Source = IsEggs ? "done.png" : "Undone.png";
                            });
            BtnDairyProduct = await AddCheckbox("Dairy", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsDairy = !IsDairy;
                img.Source = IsDairy ? "done.png" : "Undone.png";
                
            });

            BtnNuts = await AddCheckbox("Nuts", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsNuts = !IsNuts;
                img.Source = IsNuts ? "done.png" : "Undone.png";

            });

            BtnBeans = await AddCheckbox("Beans", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsBeans = !IsBeans;
                img.Source = IsBeans ? "done.png" : "Undone.png";

            });

            BtnTofu = await AddCheckbox("Tofu (soy)", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                IsTofu = !IsTofu;
                img.Source = IsTofu ? "done.png" : "Undone.png";

            });

            await AddOptions("Continue", async (sender, ee) =>
            {
                if (!IsFish && !IsEggs && !IsDairy && !IsTofu && !IsRedMeat && !IsPoultry && !IsNuts && !IsBeans)
                    return;
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("Please check your internet connection.", "Internet error");
                    return;
                }
                var str = "";
                //if (IsRedMeat)
                //    str = "Red meat";

                if (IsRedMeat)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = "Red meat";
                    }
                    else
                    {
                        str += ", Red meat";
                    }
                }

                if (IsPoultry)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = "Poultry";
                    }
                    else
                    {
                        str += ", Poultry";
                    }
                }

                if (IsFish)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        str = "Fish";
                    }
                    else
                    {
                        str += ", Fish";
                    }
                }
                if (IsEggs)
                {
                    if (string.IsNullOrEmpty(str))
                        str = "Eggs";
                    else
                        str += ", Eggs";
                }
                if (IsDairy)
                {
                    if (string.IsNullOrEmpty(str))
                        str = "Dairy";
                    else
                        str += ", Dairy";
                }
                if (IsNuts)
                {
                    if (string.IsNullOrEmpty(str))
                        str = "Nuts";
                    else
                        str += ", Nuts";
                }
                if (IsBeans)
                {
                    if (string.IsNullOrEmpty(str))
                        str = "Beans";
                    else
                        str += ", Beans";
                }
                if (IsTofu)
                {
                    if (string.IsNullOrEmpty(str))
                        str = "Tofu (soy)";
                    else
                        str += ", Tofu (soy)";
                }


                if (!string.IsNullOrEmpty(str))
                {
                    var array = str.Split(',');
                    if (array.Count() > 0)
                    {
                        str = str.Replace($", {array.Last()}", $", and{array.Last()}");
                    }
                    str = $"{str}";
                }

                await AddAnswer(str);
                VegetarianEats = str;
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                ClearOptions();
                AskForSimpleMeal();
            });
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
        }

        private async void AskForSimpleMeal()
        {
            IsSimpleMealPlan = false;
            await AddQuestion("Would you like to save time? By eating more of the same meals. Simple meals, fewer ingredients.");
            
           
            var btn2 = new DrMuscleButton()
            {
                Text = "No, more ingredients",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn2.Clicked += async (o, ev) => {
                
                await AddAnswer("No, more ingredients");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);

                ClearOptions();

                AnyAllergies();
            };
            stackOptions.Children.Add(btn2);

            await AddOptions("Yes, simple meals", async (sender, ee) =>
            {
                if (string.IsNullOrEmpty(FavouriteFood))
                    return;
                await AddAnswer("Yes, simple meals");
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);

                IsSimpleMealPlan = true;
                ClearOptions();
                
                AnyAllergies();
                
            });
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
        }


        private async void AnyAllergies()
        {

            await AddQuestion("Any allergies?");


            var btn2 = new DrMuscleButton()
            {
                Text = "Yes, allergies",
                TextColor = Color.FromHex("#195377"),
                BackgroundColor = Color.Transparent,
                HeightRequest = 55,
                BorderWidth = 2,
                BorderColor = AppThemeConstants.BlueColor,
                Margin = new Thickness(25, 2),
                CornerRadius = 0
            };
            btn2.Clicked += async (o, ev) => {

                await AddAnswer("Yes, allergies");
                IsAnyAllergies = true;
                var alleryPp = new MealGeneralPopup();
                alleryPp.SetPopupTitle("Enter allergies", GeneralPopupEnum.Allergy, "", "List all allergies");
                IsBodyweightPopup = true;
                PopupNavigation.Instance.PushAsync(alleryPp);
                ClearOptions();
            };
            stackOptions.Children.Add(btn2);


            await AddOptions("No allergies", async (sender, e) => {
                await AddAnswer("No allergies");
                IsAnyAllergies = false;
                AllergyText = "";
                HowManyExercise();
                ClearOptions();
            });

            //await AddOptions("Yes, allergies", async (sender, e) => {
            //    await AddAnswer("Yes, allergies");
            //    IsAnyAllergies = true;
            //    var alleryPp = new MealGeneralPopup();
            //    alleryPp.SetPopupTitle("Enter allergies", GeneralPopupEnum.Allergy, "", "List all allergies");
            //    IsBodyweightPopup = true;
            //    PopupNavigation.Instance.PushAsync(alleryPp);
            //    ClearOptions();
            //});
            await Task.Delay(300);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
        }

        private async void HowManyExercise()
        {
            ExericseTime = "";
            await AddQuestion("How many times a week do you exercise?");
            BtnIdontExercise = await AddCheckbox("I don't exercise", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ExericseTime = "I don't exercise";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn12Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn34Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn5PlusTimes).Content).Children[0]).Source = "Undone.png";
            });
            Btn12Times = await AddCheckbox("1-2 times", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ExericseTime = "1-2 times";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn34Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn5PlusTimes).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIdontExercise).Content).Children[0]).Source = "Undone.png";

            });

            Btn34Times = await AddCheckbox("3-4 times", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ExericseTime = "3-4 times";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn12Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn5PlusTimes).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIdontExercise).Content).Children[0]).Source = "Undone.png";
            });
            Btn5PlusTimes = await AddCheckbox("5+ times", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ExericseTime = "5+ times";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn12Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)Btn34Times).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIdontExercise).Content).Children[0]).Source = "Undone.png";
            });
            
            await AddOptions("Continue", async (sender, ee) =>
            {
                if (string.IsNullOrEmpty(ExericseTime))
                    return;
                await AddAnswer(ExericseTime);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                ClearOptions();
                HowActive();
            });
            
            
        }

        private async void HowActive()
        {
            ActiveOnJob = "";
            await AddQuestion("How active is your job (or daily occupation)?");
            BtnIMostlySit = await AddCheckbox("I mostly sit", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ActiveOnJob = "I mostly sit";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIMostlyStandWalk).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIdoManualOrPhysicalWork).Content).Children[0]).Source = "Undone.png";

            });

            BtnIMostlyStandWalk = await AddCheckbox("I mostly stand or walk", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ActiveOnJob = "I mostly stand or walk";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIMostlySit).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIdoManualOrPhysicalWork).Content).Children[0]).Source = "Undone.png";
            });
            BtnIdoManualOrPhysicalWork = await AddCheckbox("I do manual or physical work", (sender, ev) =>
            {
                Image img = (Xamarin.Forms.Image)((StackLayout)sender).Children[0];
                img.Source = "done.png";
                ActiveOnJob = "I do manual or physical work";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIMostlySit).Content).Children[0]).Source = "Undone.png";
                ((Xamarin.Forms.Image)((StackLayout)((CustomImageButton)BtnIMostlyStandWalk).Content).Children[0]).Source = "Undone.png";
            });

            await AddOptions("Continue", async (sender, ee) =>
            {
                if (string.IsNullOrEmpty(ActiveOnJob))
                    return;
                if (!CrossConnectivity.Current.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("Please check your internet connection", "Internet error");
                    return;
                }
                await AddAnswer(ActiveOnJob);
                if (Device.RuntimePlatform.Equals(Device.Android))
                    await Task.Delay(300);
                ClearOptions();
                SyncWithServer();
            });
            
        }

        private async void SyncWithServer()
        {
            UserDialogs.Instance.ShowLoading();

            try
            {


            var mealPlan = await DrMuscleRestClient.Instance.AddMealPlanAsync(new DmmMealPlan()
            {
                Id = Guid.NewGuid(),
                IsAnyAllergies = IsAnyAllergies,
                Allergies = AllergyText,
                FavoriteDiet = FavouriteFood,
                ExercisePerWeek = ExericseTime,
                ActiveOnJob = ActiveOnJob,
                VegetarianOptions = VegetarianEats,
                IsSimpleMeal = IsSimpleMealPlan
            });
                var userInfo = await DrMuscleRestClient.Instance.GetTargetIntake();
                if (userInfo?.TargetIntake != null)
                {
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
                }
                UserDialogs.Instance.HideLoading();
                if (mealPlan != null)
                {
                    await AddAnswer($"Success! Please check your email inbox");

                    LoadSavedWeightFromServer();
                    var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                    var modalPage = new Views.GeneralPopup("email.png", "Success!", "Please check your email. Plan reviewed manually in 1-2 days to meet our high standards.", "Open email", null, false, false, "false", "false", "false", "true");
                    modalPage.OkButtonPress += ModalPage_OkButtonPress1;
                    modalPage.Disappearing += (sender2, e2) =>
                    {
                        waitHandle.Set();
                    };
                    //modalPage.OkButtonPress += ModalPage_OkButtonPress;
                    await PopupNavigation.Instance.PushAsync(modalPage);

                    await Task.Run(() => waitHandle.WaitOne());

                    _firebase.LogEvent("meal_plan_order", null);
                    
                    MainGrid.IsVisible = false;
                    mainScroll.IsVisible = true;
                }
                if (userInfo?.TargetIntake != null)
                    LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
               
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
            }

            //var btn1 = new Button()
            //{
            //    Text = "Home",
            //    TextColor = Color.FromHex("#195377"),
            //    BackgroundColor = Color.Transparent,
            //    HeightRequest = 55,
            //    BorderWidth = 2,
            //    BorderColor = AppThemeConstants.BlueColor,
            //    Margin = new Thickness(25, 0)
            //};
            //btn1.Clicked += BtnHome_Clicked;
            //stackOptions.Children.Add(btn1);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //

            //var btn2 = new DrMuscleButton()
            //{
            //    Text = "Got it",
            //    TextColor = Color.FromHex("#195377"),
            //    BackgroundColor = Color.Transparent,
            //    HeightRequest = 55,
            //    BorderWidth = 2,
            //    BorderColor = AppThemeConstants.BlueColor,
            //    Margin = new Thickness(25, 2),
            //    CornerRadius = 0
            //};
            //btn2.Clicked += async (o, ev) => {
            //    MainGrid.IsVisible = false;
            //    mainScroll.IsVisible = true;
            //};
            //stackOptions.Children.Add(btn2);

            //await AddOptions("Open email", async (sender, e) =>
            //{
            //    DependencyService.Get<IOpenManager>().openMail();
            //});
            //await Task.Delay(300);
            //lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            //}
        }

        private void ModalPage_OkButtonPress1(object sender, EventArgs e)
        {
            DependencyService.Get<IOpenManager>().openMail();
        }

        private async void BtnHome_Clicked(object sender, EventArgs args)
        {
            BotList.Clear();
            ClearOptions();
            StartSetup();
        }

        private async void AskForNextMeal()
        {
            if (string.IsNullOrEmpty(LocalDBManager.Instance.GetDBSetting("email")?.Value))
                return;
            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = $"In how many hours is your next meal?",
                
                Placeholder = "Enter how many",
                OkText = "Save",
                MaxLength = 4,
                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                OnAction = async (weightResponse) =>
                {
                    if (weightResponse.Ok)
                    {
                        if (string.IsNullOrWhiteSpace(weightResponse.Value) || Convert.ToDecimal(weightResponse.Value, CultureInfo.InvariantCulture) <= 0)
                        {
                            AskForNextMeal();
                            return;
                        }

                        decimal reps = Convert.ToDecimal(weightResponse.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                        if (reps > 24)
                        {
                            AlertConfig alert = new AlertConfig()
                            {
                                Message = "Enter valid hours, should be less then 24",
                                OkText = "Ok",
                                Title = "Error",
                                AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            };
                            await UserDialogs.Instance.AlertAsync(alert);
                            // AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                            AskForNextMeal();
                            return;
                        }
                        _hour = reps;
                        SetNextMealPlan(reps);
                    }
                    else
                    {
                        BotList.Clear();
                        FabImage.IsVisible = true;
                    }
                       // AskForNextMeal();

                }
            };
            firsttimeExercisePopup.OnTextChanged += ExerciseRepsPopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);
        }

        protected void ExerciseRepsPopup_OnTextChanged(PromptTextChangedArgs obj)
        {
            const string textRegex = @"^\d+(?:[\.,]\d{0,5})?$";
            bool IsValid = Regex.IsMatch(obj.Value, textRegex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            if (IsValid == false && !string.IsNullOrEmpty(obj.Value))
            {
                double result;
                obj.Value = obj.Value.Substring(0, obj.Value.Length - 1);
                double.TryParse(obj.Value, out result);
                obj.Value = result.ToString();
            }
        }

        private async void SetNextMealPlan(decimal hours)
        {
            try
            {

                await AddAnswer(hours.ToString());

                var str = "hour";
                if (hours > 1)
                    str = "hours";
                //NotificationCenter.Current.Cancel(101);
                //var notification = new NotificationRequest
                //{
                //    NotificationId = 100,
                //    Title = "Eve Diet Coach",
                //    Description = "Time to log your meal!",
                //    ReturningData = "MealInfo", 
                //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
                //    Schedule = { NotifyTime = DateTime.Now.AddSeconds((double)hours * 60 * 60) }// Used for Scheduling local notification, if not specified notification will show immediately.
                //};
                //NotificationCenter.Current.Show(notification);
                //var notification1 = new NotificationRequest
                //{
                //    NotificationId = 101,
                //    Title = "Eve Diet Coach",
                //    Description = "Did you forget to log your meal?",
                //    ReturningData = "MealInfo", // Returning data when tapped on notification.
                //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
                //    Schedule = { NotifyTime = DateTime.Now.AddSeconds(((double)hours * 60 * 60) + (1 * 60 * 60)) } // Used for Scheduling local notification, if not specified notification will show immediately.
                //};
                //NotificationCenter.Current.Show(notification1);

                //var notification2 = new NotificationRequest
                //{
                //    NotificationId = 102,
                //    Title = "Eve Diet Coach",
                //    Description = "Did you forget to log your meal?",
                //    ReturningData = "MealInfo", 
                //    Android = { IconSmallName = { ResourceName = "eve_notification" } },
                //    Schedule = { NotifyTime = DateTime.Now.AddSeconds(((double)hours * 60 * 60) + (0.5 * 60 * 60)) } // Used for Scheduling local notification, if not specified notification will show immediately.
                //};
                //NotificationCenter.Current.Show(notification2);
                await AddQuestion($"OK, great—see you in {_hour} {str}!");
                FabImage.IsVisible = true;

            }
            catch (Exception ex)
            {

            }

        }

        async Task AddQuestion(string question, bool isAnimated = true)
        {
            BotList.Add(new BotModel()
            {
                Question = question,
                Type = BotType.Ques
            });
            if (isAnimated)
            {
                await Task.Delay(300);
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            });
        }
        void lstChats_ItemAppearing(System.Object sender, Xamarin.Forms.ItemVisibilityEventArgs e)
        {
        }
        async Task AddAnswer(string answer, bool isClearOptions = true)
        {
            BotList.Add(new BotModel()
            {
                Answer = answer,
                Type = BotType.Ans
            });

            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
            lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);

            await Task.Delay(300);
        }
        async Task<CustomImageButton> AddCheckbox(string title, EventHandler handler, bool ischecked = false)
        {
            CustomImageButton imgBtn = new CustomImageButton()
            {
                Text = title,
                Source = ischecked ? "done.png" : "Undone.png",
                BackgroundColor = Color.White,
                TextFontColor = AppThemeConstants.OffBlackColor,
                Margin = new Thickness(25, 1),
                Padding = new Thickness(2)
            };
            imgBtn.Clicked += handler;
            stackOptions.Children.Add(imgBtn);
            return imgBtn;
        }

        async Task<Grid> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() { OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90, CornerRadius = 0, HeightRequest = 55, Margin = new Thickness(25, 2) };

            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });
            grid.Children.Add(pancakeView);


            var btn = new Button()
            {
                Text = title,
                TextColor = Color.White,
                BackgroundColor = Color.Transparent,
                FontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 17,
                BorderWidth = 0,
                CornerRadius = 6,
                Margin = new Thickness(25, 5, 25, 5),
                FontAttributes = FontAttributes.Bold,
                BorderColor = Color.Transparent,
                HeightRequest = 50
            };
            btn.Clicked += handler;

            grid.Children.Add(btn);
            stackOptions.Children.Add(grid);

            BottomViewHeight.Height = GridLength.Auto;
            if (BotList.Count > 0)
            {
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.MakeVisible, false);
                lstChats.ScrollTo(BotList.Last(), ScrollToPosition.End, false);
            }
            return grid;
        }

        void ToolbarItem_Clicked(System.Object sender, System.EventArgs e)
        {
           
        }

        void NewTapped(object sender, System.EventArgs e)
        {
            ActionStack.IsVisible = !ActionStack.IsVisible;
        }

        private async Task ClearOptions()
        {
            //if (Device.RuntimePlatform.Equals(Device.iOS))
            //{
            //    stackOptions.Children.Clear();
            //    return;
            //}
            var count = stackOptions.Children.Count;
            for (var i = 0; i < count; i++)
            {
                stackOptions.Children.RemoveAt(0);
            }
            BottomViewHeight.Height = 5;
        }

        private async void UpdateSubscriptionData(SubscriptionModel subscription)
        {
            try
            {

                subscription.Email = LocalDBManager.Instance.GetDBSetting("email").Value;
                BooleanModel m = await DrMuscleRestClient.Instance.SubscriptionDetail(subscription);
                System.Diagnostics.Debug.WriteLine($"New Subscriptions added: {m.Result}");
            }
            catch (Exception)
            {

            }
        }

        private async void AddSubscriptionDataIfNotExist(SubscriptionModel subscription)
        {
            try
            {
                subscription.Email = LocalDBManager.Instance.GetDBSetting("email").Value;
                
                
            }
            catch (Exception ex)
            {

            }
        }
        //Setup Weight cards

        private async Task LoadSavedWeights()
        {
            try
            {
                
                var chartSerie = new ChartSerie() { Name = "Weight chart", Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();
                List<ChartEntry> entries = new List<ChartEntry>();
                var weightList = ((App)Application.Current).weightContext?.Weights;
                if (weightList != null)
                {
                    SetupWeightTracker(weightList);
                    if (weightList.Count < 2)
                    {
                        ImgWeight.IsVisible = true;
                        LblWeightGoal2.Text = LblWeightGoal;
                        LblWeightGoal2.FontSize = 15;
                        LblWeightGoal2.TextColor = Color.FromHex("#AA000000");
                        LblWeightGoal2.FontAttributes = FontAttributes.None;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 20);
                        LblTrackin2.IsVisible = true;
                        chartViewWeight.IsVisible = false;
                        WeightArrowText.IsVisible = false;
                        //WeightBox.IsVisible = false;
                        WeightBox2.IsVisible = false;
                    }
                    else
                    {
                        WeightArrowText.IsVisible = true;
                        ImgWeight.IsVisible = false;
                        chartViewWeight.IsVisible = true;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                        WeightArrowText.Margin = new Thickness(20, 11, 20, 20);
                        //if (weightList.Count < 4)
                        chartViewWeight.Margin = new Thickness(Device.Android == Device.RuntimePlatform ? -90 : -83, 0);

                        if (weightList.Count < 3)
                        {
                            weightList.Add(weightList.Last());
                        }


                        LblTrackin2.IsVisible = false;
                        //StackWeightProgress.IsVisible = true;
                        //Green
                        //WeightArrowText.TextColor = Color.FromHex("#5CD196");
                        WeightArrowText.Text = "0%";
                        LblWeightGoal2.FontSize = 20;
                        LblWeightGoal2.TextColor = Color.Black;
                        LblWeightGoal2.FontAttributes = FontAttributes.Bold;
                        LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                        WeightArrowText.Margin = new Thickness(20, 11, 20, 20);
                        WeightArrowText.Text = "Since last entry.";
                        if (Math.Round(weightList[0].Weight, 2) == Math.Round(weightList[1].Weight, 2))
                        {
                            LblWeightGoal2.Text = "Your weight is stable";
                        }
                        else if (Math.Round(weightList[0].Weight, 2) >= Math.Round(weightList[1].Weight, 2))
                        {
                            var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;

                            // WeightArrowText.Text = "Since last entry.";
                            LblWeightGoal2.Text = String.Format("Weight up {0}{1}%", "", Math.Round(progress)).ReplaceWithDot();


                        }
                        else
                        {
                            //Red
                            //WeightArrowText.TextColor = Color.FromHex("#BA1C31");
                            var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;
                            //WeightArrowText.Text = "Since last entry.";
                            LblWeightGoal2.Text = String.Format("Weight down {0}%", Math.Round(progress)).ReplaceWithDot().Replace("-", "");
                        }
                        //Set Weight data
                        var days = (int)((DateTime)weightList[0].CreatedDate.Date - (DateTime)weightList[1].CreatedDate.Date).TotalDays;
                        var dayStr = days > 1 ? "days" : "day";
                        WeightArrowText.Text = $"In the last {days} {dayStr}.";

                        var last3points = weightList.Take(3).Reverse();
                        foreach (var weight in last3points)
                        {
                            var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";

                            decimal val = 0;
                            if (isKg)
                                val = Math.Round(weight.Weight, 2);
                            else
                                val = Convert.ToDecimal(Math.Round(new MultiUnityWeight((decimal)weight.Weight, "kg").Lb, 2));

                            entries.Add(new ChartEntry((float)val) { Label = weight.CreatedDate.ToString("MMM dd"), ValueLabel = val.ToString() });
                        }
                        chartSerie.Entries = entries;
                        chartSeries.Add(chartSerie);

                        chartViewWeight.Chart = new LineChart
                        {
                            LabelOrientation = Orientation.Vertical,
                            ValueLabelOrientation = Orientation.Vertical,
                            LabelTextSize = 20,
                            ValueLabelTextSize = 20,
                            SerieLabelTextSize = 16,
                            LegendOption = SeriesLegendOption.None,
                            Series = chartSeries,
                        };
                    }
                }
            }
            catch { }
            LoadSavedWeightFromServer();
        }

        private async Task LoadSavedWeightFromServer()
        {
            try
            {


                var chartSerie = new ChartSerie() { Name = "Weight chart", Color = SKColor.Parse("#38418C") };
                List<ChartSerie> chartSeries = new List<ChartSerie>();
                List<ChartEntry> entries = new List<ChartEntry>();

                var weightList = await DrMuscleRestClient.Instance.GetUserWeights();

                ((App)Application.Current).weightContext.Weights = weightList;
                ((App)Application.Current).weightContext.SaveContexts();

                SetupWeightTracker(weightList);
                if (weightList.Count < 2)
                {
                    ImgWeight.IsVisible = true;
                    LblWeightGoal2.Text = LblWeightGoal;
                    LblWeightGoal2.FontSize = 15;
                    LblWeightGoal2.TextColor = Color.FromHex("#AA000000");
                    LblWeightGoal2.FontAttributes = FontAttributes.None;
                    LblWeightGoal2.Margin = new Thickness(20, 11, 20, 20);

                    chartViewWeight.IsVisible = false;
                    WeightArrowText.IsVisible = false;

                    //WeightBox.IsVisible = false;
                    WeightBox2.IsVisible = false;
                    return;
                }
                LblWeightGoal2.Margin = new Thickness(20, 11, 20, 0);
                WeightArrowText.IsVisible = true;
                ImgWeight.IsVisible = false;
                chartViewWeight.IsVisible = true;

                //if (weightList.Count < 4)
                chartViewWeight.Margin = new Thickness(-83, 0);

                if (weightList.Count < 3)
                {
                    weightList.Add(weightList.Last());
                }
                var days = (int)((DateTime)weightList[0].CreatedDate.Date - (DateTime)weightList[1].CreatedDate.Date).TotalDays;
                var dayStr = days > 1 ? "days" : "day";
                WeightArrowText.Text = $"In the last {days} {dayStr}.";

                LblTrackin2.IsVisible = false;
                //  StackWeightProgress.IsVisible = true;
                //Green
                //WeightArrowText.TextColor = Color.FromHex("#5CD196");
                WeightArrowText.FontSize = 15;
                WeightArrowText.TextColor = Color.FromHex("#AA000000");
                WeightArrowText.FontAttributes = FontAttributes.None;
                LblWeightGoal2.FontSize = 20;
                LblWeightGoal2.TextColor = Color.Black;
                LblWeightGoal2.FontAttributes = FontAttributes.Bold;
                if (Math.Round(weightList[0].Weight, 2) == Math.Round(weightList[1].Weight, 2))
                {
                    LblWeightGoal2.Text = "Your weight is stable";
                    //WeightArrowText.Text = "Since last entry.";
                }
                else if (Math.Round(weightList[0].Weight, 2) >= Math.Round(weightList[1].Weight, 2))
                {
                    var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;

                    //WeightArrowText.Text = "Since last entry.";
                    LblWeightGoal2.Text = String.Format("Weight up {0}{1}%", "", Math.Round(progress)).ReplaceWithDot();

                }
                else
                {
                    //Red
                    //WeightArrowText.TextColor = Color.FromHex("#BA1C31");
                    var progress = (weightList[0].Weight - weightList[1].Weight) * 100 / weightList[0].Weight;
                    // WeightArrowText.Text = "Since last entry.";
                    LblWeightGoal2.Text = String.Format("Weight down {0}%", Math.Round(progress)).ReplaceWithDot().Replace("-", ""); ;
                }
                //Set Weight data


                var last3points = weightList.Take(3).Reverse();
                foreach (var weight in last3points)
                {
                    var isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg";

                    decimal val = 0;
                    if (isKg)
                        val = Math.Round(weight.Weight, 2);
                    else
                        val = Convert.ToDecimal(Math.Round(new MultiUnityWeight((decimal)weight.Weight, "kg").Lb, 2));

                    entries.Add(new ChartEntry((float)val) { Label = weight.CreatedDate.ToString("MMM dd"), ValueLabel = val.ToString() });
                }
                chartSerie.Entries = entries;
                chartSeries.Add(chartSerie);

                chartViewWeight.Chart = new LineChart
                {
                    LabelOrientation = Orientation.Vertical,
                    ValueLabelOrientation = Orientation.Vertical,
                    LabelTextSize = 20,
                    ValueLabelTextSize = 20,
                    SerieLabelTextSize = 16,
                    LegendOption = SeriesLegendOption.None,
                    Series = chartSeries,
                };
            }
            catch { }
        }


        private async void SetupWeightTracker(List<UserWeight> userWeights)
        {
            try
            {

                if (userWeights == null)
                    return;
                if (userWeights.Count == 0)
                {
                    return;
                }
                bool isKg = LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg" ? true : false;
                decimal _userBodyWeight = 0;
                if (LocalDBManager.Instance.GetDBSetting("BodyWeight")?.Value != null)
                {
                    _userBodyWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.ReplaceWithDot(), CultureInfo.InvariantCulture);
                    Config.CurrentWeight = _userBodyWeight.ToString();
                }

                decimal _targetIntake = 0;
                if (LocalDBManager.Instance.GetDBSetting("TargetIntake")?.Value != null)
                {
                    _targetIntake = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("TargetIntake").Value.ReplaceWithDot(), CultureInfo.InvariantCulture);


                }
                var startWeight = Convert.ToDecimal(userWeights.Last().Weight, CultureInfo.InvariantCulture);

                var CurrentWeight = _userBodyWeight;

                decimal goalWeight = 0;

                if (LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value != null)
                {
                    goalWeight = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal")?.Value.Replace(",", "."), CultureInfo.InvariantCulture);
                    btnUpdateGoal2.IsVisible = false;
                    btnUpdateMealPlan.IsVisible = true;
                }
                else
                {
                    btnUpdateGoal2.IsVisible = true;
                    btnUpdateMealPlan.IsVisible = false;
                    LblWeightTip2.Text = "Goal weight not set";
                    LblWeightTipText2.Text = "";// "Update your goal weight to see the weight tips here.";
                }
                if (_targetIntake == 0)
                {
                    var userInfo = await DrMuscleRestClient.Instance.GetTargetIntakeWithoutLoader();
                    if (userInfo?.TargetIntake != null)
                    {
                        LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
                        _targetIntake = (decimal)userInfo.TargetIntake;
                    }
                }
                var togoOfGoal = "";
                if (goalWeight == 0)
                {
                    if (_targetIntake != 0)
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day";
                    else
                        LblTargetIntake2.Text = $"Calories not set";
                    btnUpdateGoal.IsVisible = true;
                    btnMealPlan.IsVisible = false;
                }
                else
                {
                    if (CurrentWeight < goalWeight)
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day to build muscle";
                    else
                        LblTargetIntake2.Text = $"{Math.Round(_targetIntake)} cal/day to lose fat";
                    btnUpdateGoal.IsVisible = false;
                    btnMealPlan.IsVisible = true;
                }
                
                
               
                if (isKg)
                {

                    LblCurrentText2.Text = string.Format("{0:0.##} {1}", Math.Round(CurrentWeight, 2), "kg");

                    LblGoalText2.Text = goalWeight == 0 ? "?" : string.Format("{0:0.##} {1}", Math.Round(goalWeight, 2), "kg");
                    togoOfGoal = goalWeight == 0 ? "?" : string.Format("{0:0.##}", Math.Round(goalWeight, 2));
                    LblStartText2.Text = string.Format("{0:0.##} {1}", Math.Round(startWeight, 2), "kg");


                }
                else
                {
                    LblCurrentText2.Text = string.Format("{0:0.##} {1}", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2), "lbs");

                    LblGoalText2.Text = goalWeight == 0 ? "?" : string.Format("{0:0.##} {1}", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2), "lbs");

                    togoOfGoal = goalWeight == 0 ? "?" : string.Format("{0:0.##}", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2));
                    LblStartText2.Text = string.Format("{0:0.##} {1}", Math.Round(new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2), "lbs");
                }
                
                bool isGain = false;
                if (CurrentWeight < goalWeight)
                {
                    isGain = true;

                }
                if (goalWeight != 0)
                {
                    string Gender = LocalDBManager.Instance.GetDBSetting("gender").Value.Trim();
                    var creationDate = new DateTime(Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("creation_date").Value));
                    decimal weeks = 0;
                    if (creationDate != null)
                    {
                        weeks = (int)(DateTime.Now - creationDate).TotalDays / 7;
                    }
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
                    decimal rate = (decimal)2.3;
                    if (result.Contains("build muscle and burn fat"))
                    {
                        rate = (decimal)2.4;
                    }
                    else if (result.Contains("build muscle"))
                    {
                        rate = (decimal)2.3;
                    }
                    else if (result.Contains("burn fat"))
                    {
                        rate = (decimal)2.4;
                    }
                    decimal gainDouble = 0;
                    if (Gender == "Man")
                    {
                        if (weeks <= 18)
                            gainDouble = ((decimal)0.015 - (decimal)0.000096899 * weeks) * CurrentWeight;
                        else if (weeks > 18 && weeks <= 42)
                            gainDouble = ((decimal)0.011101 - (decimal)0.000053368 * weeks) * CurrentWeight;
                        else if (weeks > 42)
                            gainDouble = (decimal)0.00188 * CurrentWeight;
                    }
                    else
                    {
                        if (weeks <= 18)
                            gainDouble = (((decimal)0.015 - (decimal)0.000096899 * weeks) * CurrentWeight) / 2;
                        else if (weeks > 18 && weeks <= 42)
                            gainDouble = (((decimal)0.011101 - (decimal)0.000053368 * weeks) * CurrentWeight) / 2;
                        else if (weeks > 42)
                            gainDouble = ((decimal)0.00188 * CurrentWeight) / 2;
                    }
                    //Convert to day
                    gainDouble = gainDouble / 30;


                    decimal loseDouble = ((decimal)0.01429 * CurrentWeight) / 30;


                    string gain = string.Format("{0:0.##}", isKg ? Math.Round(gainDouble, 2) : Math.Round(new MultiUnityWeight(gainDouble, WeightUnities.kg).Lb, 2));

                    string lose = string.Format("{0:0.##}", isKg ? Math.Round(loseDouble, 2) : Math.Round(new MultiUnityWeight(loseDouble, WeightUnities.kg).Lb, 2));
                    var weekText = weeks <= 1 ? "week" : "weeks";
                    int days = 0;

                    if (userWeights.Count > 1)
                    {
                        days = Math.Abs((int)(userWeights[1].CreatedDate.Date - userWeights.First().CreatedDate.Date).TotalDays);
                        startWeight = Convert.ToDecimal(userWeights[1].Weight, CultureInfo.InvariantCulture);
                    }

                    double totalChanged = 0;
                    if (userWeights.Count > 1)
                        totalChanged = (double)(((userWeights.First().Weight - userWeights[1].Weight) * 100) / userWeights[1].Weight);
                    double dailyChanged = (double)totalChanged;

                    if (days != 0)
                        dailyChanged = totalChanged / days;
                    bool isLess = false;
                    if (days == 0)
                        days = 1;
                    if (CurrentWeight > goalWeight)
                    {
                        //Lose weight
                        if (Math.Round(CurrentWeight, 1) >= Math.Round(startWeight, 1))
                        {
                            isLess = true;
                        }
                        else
                        {
                            if (loseDouble > (Math.Abs((startWeight - CurrentWeight) / days)))
                                isLess = true;
                            else
                                isLess = false;

                        }
                    }
                    else
                    {
                        //Gain
                        if (Math.Round(CurrentWeight, 1) <= Math.Round(startWeight, 1))
                        {
                            isLess = false;
                        }
                        else
                        {
                            if (gainDouble < (Math.Abs((startWeight - CurrentWeight) / days)))
                                isLess = true;
                            else
                                isLess = false;
                        }

                    }

                    var lessMoreText = "";

                    if (CurrentWeight <= goalWeight)
                    {
                        //Gain weight
                        if (isLess)
                        {
                            lessMoreText = "so you're probably gaining fat.";//$"You're probably gaining fat. Eat less (but aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                        else
                        {

                            lessMoreText = $"so you could speed that up by eating more."; //$"You're probably leaving muscle on the table. Eat more (and aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                    }
                    else
                    {
                        //lose weight
                        if (isLess)
                        {
                            //lessMoreText = $"you could speed that up by eating less. And aim to eat {Math.Round(CurrentWeight * rate)} g of protein a day.";
                            lessMoreText = "so you could speed that up by eating less.";//$"To speed that up, eat less (but aim for {Math.Round(CurrentWeight * rate)} g protein / day).";

                        }
                        else
                        {
                            //lessMoreText = $"you're probably losing muscle mass too. Eat more to prevent that. And aim to eat {Math.Round(CurrentWeight * rate)} g of protein a day.";
                            lessMoreText = "so you're probably losing muscle mass.";//$"You're probably losing muscle mass. Eat more (and aim for {Math.Round(CurrentWeight * rate)} g protein / day).";
                        }
                    }

                    var goalGainWeight = string.Format("{0:0.##}", Math.Round(CurrentWeight * rate) / 1000, 2);


                    var gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(CurrentWeight - startWeight, 2)));
                    var gainInaMonth = Math.Round(CurrentWeight - startWeight, 2) / days;
                    var gainInaMonthText = string.Format("{0:0.##}", Math.Round(Math.Abs((CurrentWeight - startWeight)) / days, 2));
                    var gainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(goalWeight - startWeight, 2)));
                    var remainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(goalWeight - CurrentWeight, 2)));
                    var massunit = "kg";
                    if (!isKg)
                    {
                        massunit = "lbs";
                        gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                        gainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                        remainDiffernece = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2)));
                        goalGainWeight = string.Format("{0:0.##}", Math.Round(new MultiUnityWeight(CurrentWeight * rate, "kg").Lb / (decimal)453.59237, 2));

                        gainInaMonthText = string.Format("{0:0.##}", Math.Abs(Math.Round((new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb) / days, 2)));
                    }

                    if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1) && Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                    {
                        LblWeightTip2.Text = "Your weight is stable";
                        LblWeightTipText2.Text = $"At {LblCurrentText2.Text}, your weight is stable. Aim for {Math.Round(CurrentWeight * rate)} g protein / day.";
                        LblWeightToGo2.Text = string.Format("Success! {0} 💪", LblGoalText2.Text);
                    }
                    else if (Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                    {

                        LblWeightTip2.Text = string.Format("{0} {1} {2} in {3} {4}", gainWeight, massunit, CurrentWeight > startWeight ? "gained" : "lost", days, days > 1 ? "days" : "day");
                        LblWeightToGo2.Text = string.Format("Success! {0} 💪", LblGoalText2.Text);

                        LblWeightTipText2.Text = $"At {LblCurrentText2.Text}, you're at your goal weight. Aim for {Math.Round(CurrentWeight * rate)} g protein / day.";

                    }
                    else if (CurrentWeight < goalWeight)
                    {
                        //Gain weight



                        if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                        {
                            LblWeightTip2.Text = "Your weight is stable";
                            // LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";
                            LblWeightTipText2.Text = $"Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. your weight is stable. {lessMoreText}";
                        }
                        else if (CurrentWeight > startWeight)
                        {

                            LblWeightTip2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //   LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're gaining {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                        }
                        else if (CurrentWeight < startWeight)
                        {
                            LblWeightTip2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            // LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a month. Currently, you're losing {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                            LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. Since you have been with us for {weeks} {weekText}, you should gain about {gain} {massunit} a day. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                        }
                    }
                    else
                    {
                        //Loose weight
                        if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                        {
                            LblWeightTip2.Text = "Your weight is stable";
                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            //    LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should gain about {lose} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";

                            //  LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should gain about {lose} {massunit} a month. Currently, you're at your starting weight. So, {lessMoreText}";
                            LblWeightTipText2.Text = $"At {LblCurrentText2.Text}, you should lose about {lose} {massunit} a day. your weight is stable. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //   LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";

                        }
                        else if (CurrentWeight > startWeight)
                        {

                            LblWeightTip2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a month. Currently, you're gaining {gainInaMonthText} {massunit} a month. So, {lessMoreText}";

                            LblWeightTipText2.Text = $"You're gaining {gainInaMonthText} {massunit} a day. At {LblCurrentText2.Text}, you should lose about {lose} {massunit} a day. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";
                        }
                        else if (CurrentWeight < startWeight)
                        {
                            LblWeightTip2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");

                            LblWeightToGo2.Text = $"{remainDiffernece} {massunit} to goal of {togoOfGoal}";

                            LblWeightTipText2.Text = $"You're losing {gainInaMonthText} {massunit} a day. At {LblCurrentText2.Text}, you should lose about {lose} {massunit} a day. {lessMoreText}";
                            LblWeightTipText2.Text = LblWeightTipText2.Text.Replace("so ", "So, ");
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. At {LblCurrentText1.Text}, you should lose about {lose} {massunit} a month. Currently, you're losing {gainInaMonthText} {massunit} a month. So, {lessMoreText}";
                            //LblWeightTipText2.Text = $"{remainDiffernece} {massunit} to go. To know if you're eating enough calories, track your waist circumference every week. If it's going up, {lessMoreText}";
                        }

                    }
                }
                else
                {
                    int days = 0;
                    if (userWeights.Count > 1)
                    {
                        days = Math.Abs((int)(userWeights[1].CreatedDate.Date - userWeights.First().CreatedDate.Date).TotalDays);
                        startWeight = Convert.ToDecimal(userWeights[1].Weight, CultureInfo.InvariantCulture);
                    }


                    if (days == 0)
                        days = 1;
                    var gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(CurrentWeight - startWeight, 2)));
                    var gainInaMonth = Math.Round(CurrentWeight - startWeight, 2) / days * 30;

                    var massunit = "kg";
                    if (!isKg)
                    {
                        massunit = "lbs";
                        gainWeight = string.Format("{0:0.##}", Math.Abs(Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2)));

                    }
                    if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                    {
                        LblWeightToGo2.Text = "Your are at your starting weight";

                        LblWeightTipText2.Text = $"";
                    }
                    else if (CurrentWeight > startWeight)
                    {
                        LblWeightToGo2.Text = string.Format("{0} {1} gained in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                    }
                    else if (CurrentWeight < startWeight)
                    {
                        LblWeightToGo2.Text = string.Format("{0} {1} lost in {2} {3}", gainWeight, massunit, days, days > 1 ? "days" : "day");
                    }
                }
                //LblWeightToGo1.Text = LblWeightToGo2.Text;
                //LblWeightTip1.Text = LblWeightTip2.Text;
                //LblWeightTipText1.Text = LblWeightTipText2.Text;
                if (Math.Round(CurrentWeight, 1) == Math.Round(startWeight, 1))
                {
                    LbltrackerText2.Text = $"Your are at your starting weight";
                    
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    
                }

                else if (Math.Round(CurrentWeight, 1) == Math.Round(goalWeight, 1))
                {
                    if (isKg)
                        LbltrackerText2.Text =
                            string.Format("Success! {0:0.##} kg 💪", Math.Round(CurrentWeight, 2));
                    else
                        LbltrackerText2.Text = string.Format("Success! {0:0.##} lbs 💪", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));
                    
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    
                }
                else if (CurrentWeight > goalWeight && goalWeight > startWeight)
                {
                    //Progress smoothly
                    
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    if (isKg)
                    {
                        LbltrackerText2.Text = string.Format("Success! {0:0.##} kg above goal", Math.Round(CurrentWeight - goalWeight, 2));

                        
                    }
                    else
                    {
                        LbltrackerText2.Text = string.Format("Success! {0:0.##} lbs above goal", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)goalWeight, "kg").Lb, 2));
                        
                    }
                }
                else if (CurrentWeight < goalWeight && goalWeight < startWeight)
                {
                    //Progress smoothly
                    
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    if (isKg)
                    {
                        LbltrackerText2.Text = string.Format("Success! {0:0.##} kg under goal", Math.Round(goalWeight - CurrentWeight, 2));

                        //LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                    else
                    {
                        LbltrackerText2.Text = string.Format("Success! {0:0.##} lbs under goal", Math.Round(new MultiUnityWeight((decimal)goalWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));
                        //LbltrackerText2.Text = LbltrackerText1.Text;
                    }
                }
                else if (CurrentWeight > startWeight)
                {
                    //Overweight
                    if (goalWeight < CurrentWeight)
                    {
                        LbltrackerText2.TextColor =  Color.Red;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Red;
                    }
                    else
                    {
                        LbltrackerText2.TextColor =  Color.Green;
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    }
                    if (isKg)
                    {
                        LbltrackerText2.Text = string.Format("You have gained {0:0.##} kg", Math.Round(CurrentWeight - startWeight, 2));
                    }
                    else
                    {
                        LbltrackerText2.Text = string.Format("You have gained {0:0.##} lbs", Math.Round(new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb - new MultiUnityWeight((decimal)startWeight, "kg").Lb, 2));
                    }
                }

                else if (CurrentWeight < startWeight)
                {
                    //Low weight
                    if (goalWeight < CurrentWeight)
                    {
                        
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    }
                    else
                    {
                        
                        LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Red;
                    }
                    if (isKg)
                    {
                        LbltrackerText2.Text = string.Format("You have lost {0:0.##} kg", Math.Round(startWeight - CurrentWeight, 2));
                        
                    }
                    else
                    {
                        LbltrackerText2.Text =
                 string.Format("You have lost {0:0.##} lbs", Math.Round(new MultiUnityWeight((decimal)startWeight, "kg").Lb - new MultiUnityWeight((decimal)CurrentWeight, "kg").Lb, 2));

                    }
                    //if (!isGain)
                    //{
                    //    LbltrackerText1.TextColor = FrmTracker1.BackgroundColor = Color.Green;
                    //    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;

                    //}
                }


                else if (CurrentWeight == startWeight)
                {
                    LbltrackerText2.Text = $"Your weight is stable";
                    LbltrackerText2.TextColor = FrmTracker2.BackgroundColor = Color.Green;
                    LbltrackerText2.Text = LbltrackerText2.Text;
                }

            }
            catch (Exception ex)
            {

            }
        }

        async void BtnLearnMore_Clicked(System.Object sender, System.EventArgs e)
        {
            if (CheckTrialUser())
                return;
            await PagesFactory.PushAsync<LearnPage>();
        }


        private bool CheckTrialUser()
        {
            if (App.IsFreePlan)
            {
                ConfirmConfig ShowWelcomePopUp2 = new ConfirmConfig()
                {
                    Message = "Upgrading will unlock custom coaching tips based on your goals and progression.",
                    Title = "You discovered a premium feature!",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    OkText = "Upgrade",
                    CancelText = "Maybe later",
                    OnAction = async (bool ok) =>
                    {
                        if (ok)
                        {
                            PagesFactory.PushAsync<SubscriptionPage>();
                        }
                        else
                        {

                        }
                    }
                };
                UserDialogs.Instance.Confirm(ShowWelcomePopUp2);
            }
            return App.IsFreePlan;
        }

        bool isMealPlan = false;
        async void GetMealPlan_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }
            if (isMealPlan)
                return;
            //if (App.IsMealPlan)
            //{
            isMealPlan = true;
            await PagesFactory.PushAsync<MealInfoPage>();
            isMealPlan = false;
            //}
            //else
            //{
            //    await PagesFactory.PushAsync<SubscriptionPage>();
            //}

        }

        async void EnterWeight_Clicked(System.Object sender, System.EventArgs e)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }

            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = "Update body weight",
                MaxLength = 7,
                Placeholder = "Tap to enter your weight",
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
                        var weightText = weightResponse.Value.Replace(",", ".");
                        decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);

                        LocalDBManager.Instance.SetDBSetting("BodyWeight", new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().Replace(",", "."));
                        var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("BodyWeight").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                        var weights = new MultiUnityWeight(value, "kg");
                       // LblBodyweight.Text = string.Format("{0:0.00}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weights.Kg : weights.Lb);
                        await DrMuscleRestClient.Instance.SetUserBodyWeight(new UserInfosModel()
                        {
                            BodyWeight = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                        });
                        // WeightBox.IsVisible = false;
                        if (Device.RuntimePlatform.Equals(Device.iOS))
                        {
                            IHealthData _healthService = DependencyService.Get<IHealthData>();
                            await _healthService.GetWeightPermissionAsync(async (r) =>
                            {
                                var a = r;
                                if (r)
                                {
                                    _healthService.SetWeight(LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? (double)Math.Round(weights.Kg, 2) : (double)Math.Round(weights.Lb, 2));
                                }
                            });
                        }
                        LoadSavedWeightFromServer();
                        return;
                    }
                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }

        async void btnGetSupport_Clicked(System.Object sender, System.EventArgs e)
        {

            ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).CurrentPage = ((MainTabbedPage)(global::DrMuscle.App.Current.MainPage).Navigation.NavigationStack[0]).Children[2];
        }
        async void btnUpdateGoal_Clicked(System.Object sender, System.EventArgs e)
        {

            if (!CrossConnectivity.Current.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync(new AlertConfig()
                {
                    Message = AppResources.PleaseCheckInternetConnection,
                    Title = AppResources.ConnectionError,
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray)
                });
                return;
            }

            PromptConfig firsttimeExercisePopup = new PromptConfig()
            {
                InputType = Device.RuntimePlatform.Equals(Device.Android) ? InputType.Phone : InputType.DecimalNumber,
                IsCancellable = true,
                Title = "Update goal weight",
                MaxLength = 7,
                Placeholder = "Tap to enter your goal weight",
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
                        var weightText = weightResponse.Value.Replace(",", ".");
                        decimal weight1 = Convert.ToDecimal(weightText, CultureInfo.InvariantCulture);

                        LocalDBManager.Instance.SetDBSetting("WeightGoal", new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value).Kg.ToString().Replace(",", "."));
                        var value = Convert.ToDecimal(LocalDBManager.Instance.GetDBSetting("WeightGoal").Value.Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture);
                        var weights = new MultiUnityWeight(value, "kg");
                        //LblBodyweight.Text = string.Format("{0:0.00}", LocalDBManager.Instance.GetDBSetting("massunit").Value == "kg" ? weights.Kg : weights.Lb);
                        await DrMuscleRestClient.Instance.SetUserWeightGoal(new UserInfosModel()
                        {
                            WeightGoal = new MultiUnityWeight(weight1, LocalDBManager.Instance.GetDBSetting("massunit").Value)
                        });
                        btnUpdateGoal.IsVisible = false;
                        btnMealPlan.IsVisible = true;

                        var userInfo = await DrMuscleRestClient.Instance.GetTargetIntake();
                        if (userInfo.TargetIntake != null)
                            LocalDBManager.Instance.SetDBSetting("TargetIntake", userInfo.TargetIntake.ToString());
                        LoadSavedWeightFromServer();
                        return;
                    }
                }
            };

            firsttimeExercisePopup.OnTextChanged += FirsttimeExercisePopup_OnTextChanged;
            UserDialogs.Instance.Prompt(firsttimeExercisePopup);

        }

    }
}
