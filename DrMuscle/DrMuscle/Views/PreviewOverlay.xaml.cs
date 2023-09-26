using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.User;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using static SQLite.SQLite3;

namespace DrMuscle.Views
{
    public partial class PreviewOverlay : PopupPage
    {
        public RegisterModel _registerModel { get; set; }
        bool isAILoaded = false;
        string aiTitle = "";
        string aiDescription = "";

        public PreviewOverlay()
        {
            InitializeComponent();
            string cweight = "", todayweight = "", liftedweight = "";

            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
            {
                cweight = "95 kg";
                todayweight = "120 kg";
                liftedweight = "242 kg";
            }
            else
            {
                cweight = "210 lbs";
                todayweight = "265 lbs";
                liftedweight = "535 lbs";
            }
            //LblReview2.Text = $"\"I was never that heavy ({cweight})\"";
            //LblsubHeadingReviewer2.Text = $"\"My strength on hip trusts exploded from something like {todayweight} to {liftedweight}. The app has scientific algorithms... simple stupid and effective wether its raining or sunshine, just follow the app and don't overthink to much.\"";
            //LblReviewerName2.Text = "Jonas Notter, World Natural Bodybuilding Champion";
            //Setup();


            var tapLinkTermsOfUseGestureRecognizer = new TapGestureRecognizer();
            tapLinkTermsOfUseGestureRecognizer.NumberOfTapsRequired = 1;
            tapLinkTermsOfUseGestureRecognizer.Tapped += (s, e) =>
            {
                Device.OpenUri(new Uri("http://drmuscleapp.com/news/terms/"));
            };


            DependencyService.Get<IDrMuscleSubcription>().OnMonthlyAccessPurchased += async delegate
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PagesFactory.PushAsyncWithoutBefore<MainAIPage>();
                }
                else
                    await PagesFactory.PushAsync<MainAIPage>(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };
            DependencyService.Get<IDrMuscleSubcription>().OnYearlyAccessPurchased += async delegate
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    await PagesFactory.PushAsyncWithoutBefore<MainAIPage>();
                }
                else
                    await PagesFactory.PushAsync<MainAIPage>(); MessagingCenter.Send<SubscriptionSuccessfulMessage>(new SubscriptionSuccessfulMessage(), "SubscriptionSuccessfulMessage");
            };



        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Setup();
            AnaliesAIWithChatGPT();
        }

        private async void Setup()
        {

            string goal = "build lean muscle";
            var result = "";
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("Demoreprange")?.Value == "BuildMuscle")
                {
                    goal = "build muscle";
                    result = "This helps you build muscle.";
                    LblGenderGoal4.Text = "Build muscle";
                }
                else if (LocalDBManager.Instance.GetDBSetting("Demoreprange")?.Value == "BuildMuscleBurnFat")
                {
                    goal = "build lean muscle";//"Build muscle and burn fat";
                    result = "This helps you build muscle and burn fat.";
                    LblGenderGoal4.Text = "Build muscle and burn fat";
                }
                else //if (LocalDBManager.Instance.GetDBSetting("Demoreprange").Value == "FatBurning")
                {
                    goal = "burn fat";
                    result = "This helps you burn fat.";
                    LblGenderGoal4.Text = "Burn fat ";
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                if (LocalDBManager.Instance.GetDBSetting("gender")?.Value.Trim() != "Man")
                {
                    ImgGender.Source = "ExerciseBackground";
                    
                }
            }
            catch (Exception ex)
            {

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

            string fname = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;

            DrMuscleWorkoutsButton.Text = $"{fname}, this program will help you {goal}";


            string cweight = "", todayweight = "", liftedweight = "";

            if (LocalDBManager.Instance.GetDBSetting("massunit")?.Value == "kg")
            {
                cweight = "95 kg";
                todayweight = "120 kg";
                liftedweight = "242 kg";
            }
            else
            {
                cweight = "210 lbs";
                todayweight = "265 lbs";
                liftedweight = "535 lbs";
            }
            //LblReview2.Text = $"\"I was never that heavy ({cweight})\"";
            //LblsubHeadingReviewer2.Text = $"\"My strength on hip trusts exploded from something like {todayweight} to {liftedweight}. The app has scientific algorithms... simple stupid and effective wether its raining or sunshine, just follow the app and don't overthink to much.\"";
            //LblReviewerName2.Text = "Jonas Notter, World Natural Bodybuilding Champion";


            var programName = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel")?.Value;
            List<string> listSource = new List<string>();
            if (programName == null)
                programName = "";
            //listSource.Add("View more programs");
            if (programName.ToLower().Contains("gym"))
            {
                if (_registerModel?.EquipmentModel?.IsEquipmentEnabled == true)
                {
                    if (_registerModel?.EquipmentModel?.IsDumbbellEnabled == true)
                        flChipView.Children.Add(CreateRandomBoxview("Dumbbell training "));
                    if (_registerModel?.EquipmentModel?.IsPlateEnabled == true) flChipView.Children.Add(CreateRandomBoxview("Barbell training "));
                    //if (LocalDBManager.Instance.GetDBSetting("ChinUp")?.Value == "true")
                    //    flChipView.Children.Add(CreateRandomBoxview("Chin-up training "));
                    //if (LocalDBManager.Instance.GetDBSetting("Pully")?.Value == "true")
                    //    flChipView.Children.Add(CreateRandomBoxview("Pulley training "));
                }
                else
                {
                    flChipView.Children.Add(CreateRandomBoxview("Barbell training "));
                    flChipView.Children.Add(CreateRandomBoxview("Dumbbell training "));
                }
                flChipView.Children.Add(CreateRandomBoxview("Bodyweight training"));
                if (_registerModel?.IsMobility == true)
                    flChipView.Children.Add(CreateRandomBoxview("Mobility training"));

                if (_registerModel?.IsCardio == true)
                    flChipView.Children.Add(CreateRandomBoxview("Cardio training"));

                    flChipView.Children.Add(CreateRandomBoxview("Strength training"));

                    flChipView.Children.Add(CreateRandomBoxview("Back-off sets"));
            }
            else if (programName.ToLower().Contains("bands"))
            {
                flChipView.Children.Add(CreateRandomBoxview("Bodyweight training"));
                flChipView.Children.Add(CreateRandomBoxview("Bands training"));

                if (_registerModel?.IsMobility == true)
                    flChipView.Children.Add(CreateRandomBoxview("Mobility training"));

                if (_registerModel?.IsCardio == true)
                    flChipView.Children.Add(CreateRandomBoxview("Cardio training"));
            }
            else if (programName.ToLower().Contains("bodyweight"))
            {
                flChipView.Children.Add(CreateRandomBoxview("Bodyweight training"));
                flChipView.Children.Add(CreateRandomBoxview("Bands training"));

                if (_registerModel?.IsMobility == true)
                    flChipView.Children.Add(CreateRandomBoxview("Mobility training"));

                if (_registerModel?.IsCardio == true)
                    flChipView.Children.Add(CreateRandomBoxview("Cardio training"));
            }
            else if (programName.ToLower().Contains("home"))
            {
                if (_registerModel?.EquipmentModel?.IsHomeEquipmentEnabled == true)
                {
                    if (_registerModel?.EquipmentModel?.IsHomeDumbbell == true)
                        flChipView.Children.Add(CreateRandomBoxview("Dumbbell training "));
                    if (_registerModel?.EquipmentModel?.IsHomePlate == true) flChipView.Children.Add(CreateRandomBoxview("Barbell training "));
                    
                }
                else
                {
                    flChipView.Children.Add(CreateRandomBoxview("Barbell training "));
                    flChipView.Children.Add(CreateRandomBoxview("Dumbbell training "));
                }

                flChipView.Children.Add(CreateRandomBoxview("Bodyweight training"));

                if (_registerModel?.IsMobility == true)
                    flChipView.Children.Add(CreateRandomBoxview("Mobility training"));

                if (_registerModel?.IsCardio == true)
                    flChipView.Children.Add(CreateRandomBoxview("Cardio training"));

                
                    flChipView.Children.Add(CreateRandomBoxview("Strength training"));

                
                    flChipView.Children.Add(CreateRandomBoxview("Back-off sets"));
            }



            //programPicker.ItemsSource = listSource;

        }

        void WorkoutPageRecognizer_Tapped(object sender, EventArgs e)
        {

        }

        private async Task<string> AnaliesAIWithChatGPT(bool isloader = false, double temperature = 0.7, int maxTokens = 2500, double topP = 1, double frequencyPenalty = 0, double presencePenalty = 0)
        {
            return "AI analysis";
        }


        async void Close_Tapped(object sender, EventArgs e)
        {
            try
            {
                await BoomSuccessPopup();
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    PopupNavigation.Instance.PopAllAsync();
                await OpenDemo();
            }
            catch (Exception ex)
            {

            }

        }

        async Task OpenDemo()
        {
            CurrentLog.Instance.CurrentExercise = new ExerciceModel()
            {
                BodyPartId = 7,
                VideoUrl = "https://youtu.be/Plh1CyiPE_Y",
                IsBodyweight = true,
                IsEasy = false,
                IsFinished = false,
                IsMedium = false,
                IsNextExercise = false,
                IsNormalSets = false,
                IsSwapTarget = false,
                IsSystemExercise = true,
                IsTimeBased = false,
                IsUnilateral = false,
                Label = "Crunch",
                RepsMaxValue = null,
                RepsMinValue = null,
                Timer = null,
                Id = 864
            };
            App.IsDemoProgress = true;
            LocalDBManager.Instance.SetDBSetting("DemoProgress", "true");
            await PagesFactory.PushAsync<Screens.Demo.NewDemoPage>();

        }
        void HelpGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            PagesFactory.PushAsync<FAQPage>();
        }

        void MoreUserReviewGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
        }

        void NewUpdatesGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
        {
            //
            Device.OpenUri(new Uri("https://dr-muscle.com/timeline/"));
        }

        void TapMoreExperReviews_Tapped(System.Object sender, System.EventArgs e)
        {
            Device.OpenUri(new Uri("https://dr-muscle.com/reviews/"));
        }

        private Frame CreateRandomBoxview(string items)
        {
            var view = new Frame();    // Creating New View for design as chip
            view.BackgroundColor = Color.FromHex("#AAE1E1E1");
            view.BorderColor = Color.Transparent;
            view.Padding = new Thickness(7, 7);
            view.CornerRadius = 4;
            view.HasShadow = false;


            // creating new child that holds the value of item list and add in View
            var label = new Label();
            label.Text = items;
            label.TextColor = Color.Black;
            label.HorizontalOptions = LayoutOptions.Center;
            label.VerticalOptions = LayoutOptions.Center;
            label.FontSize = 17;
            view.Content = label;
            return view;
        }

        void programPicker_PropertyChanged(object sender, EventArgs e)
        {
        }

        async void programPicker_Unfocused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            //if (programPicker != null && programPicker.SelectedIndex != -1)
            //{
            //    ConfirmConfig supersetConfig = new ConfirmConfig()
            //    {
            //        Title = "Are you sure?",
            //        Message = $"Your program will change to {programPicker.SelectedItem}",
            //        OkText = "Change program",
            //        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
            //        CancelText = AppResources.Cancel,
            //    };

            //    var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
            //    if (x)
            //    {
            //        try
            //        {

            //            var selectedProgram = (string)programPicker.SelectedItem;
            //            var programName = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel")?.Value;
            //            var level = int.Parse($"{programName.Trim().Last()}");
            //            var selectedProgramShortName = "";
            //            if (selectedProgram.Equals("Full-body"))
            //                selectedProgramShortName = "Full body";
            //            else if (selectedProgram.Equals("Upper/Lower-Body Split"))
            //                selectedProgramShortName = "Split body";
            //            else if (selectedProgram.Equals("Push/Pull/Legs Split"))
            //                selectedProgramShortName = "PPL";
            //            else if (selectedProgram.Equals("Powerlifting"))
            //                selectedProgramShortName = "Powerlifting";
            //            else if (selectedProgram.Equals("Buffed with Bands"))
            //                selectedProgramShortName = "Bands only";
            //            else if (selectedProgram.Equals("Bodyweight level 1"))
            //            {
            //                selectedProgramShortName = "Bodyweight";
            //                level = 1;
            //            }
            //            else if (selectedProgram.Equals("Bodyweight level 2"))
            //            {
            //                selectedProgramShortName = "Bodyweight";
            //                level = 2;
            //            }
            //            else if (selectedProgram.Equals("Bodyweight level 3"))
            //            {
            //                selectedProgramShortName = "Bodyweight";
            //                level = 3;
            //            }

            //            var mo = DrMuscle.Constants.AppThemeConstants.GetLevelProgram(level, programName.ToLower().Contains("gym"), selectedProgram.Equals("Full-body"), selectedProgramShortName);
            //            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
            //            LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
            //            LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
            //            LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

            //            LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);
            //            PopupNavigation.Instance.PopAsync();

            //        }
            //        catch (Exception ex)
            //        {

            //        }
            //    }
            //    else
            //    {
            //        programPicker.SelectedIndex = -1;
            //    }
            //}
        }

        async void ViewmoreProgram(object sender, EventArgs e)
        {

            var programName = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel")?.Value;
            List<string> listSource = new List<string>();
            btnViewMoreProgram.IsVisible = false;
            workoutFrm.IsVisible = true;


            if (!programName.ToLower().Contains("full-body"))
                await AddOptions("Full-body (2-4 workouts / week)", ChangedProgram);
            if (!programName.ToLower().Contains("up/low"))
                await AddOptions("Upper/lower (3-4 / week)", ChangedProgram);
            if (!programName.ToLower().Contains("push/pull/legs"))
                await AddOptions("Push/pull/legs (6 / week)", ChangedProgram);
            if (!programName.ToLower().Contains("powerlifting"))
                await AddOptions("Powerlifting (2-4 / week)", ChangedProgram);
            if (!programName.ToLower().Contains("bands"))
                await AddOptions("Bands only (2-4 / week)", ChangedProgram);
            if (!programName.ToLower().Contains("bodyweight"))
                await AddOptions("Bodyweight only (2-4 / week)", ChangedProgram);

            await Task.Delay(300);
            workoutFrm.VerticalOptions = LayoutOptions.FillAndExpand;
            workoutStack.VerticalOptions = LayoutOptions.FillAndExpand;
            scrollView.ScrollToAsync(workoutFrm, ScrollToPosition.End, true);
        }
        async void ChangedProgram(object sender, EventArgs e)
        {
            try
            {
                var selectedProgram = ((DrMuscleButton)sender).Text;
                var programName = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel")?.Value;
                if (string.IsNullOrEmpty(programName))
                    programName = "1";
                var level = int.Parse($"{programName.Trim().Last()}");
                var selectedProgramShortName = "";
                if (selectedProgram.Contains("Full-body"))
                    selectedProgramShortName = "Full body";
                else if (selectedProgram.Contains("Upper/lower"))
                    selectedProgramShortName = "Split body";
                else if (selectedProgram.Contains("Push/pull/legs"))
                    selectedProgramShortName = "PPL";
                else if (selectedProgram.Contains("Powerlifting"))
                    selectedProgramShortName = "Powerlifting";
                else if (selectedProgram.Contains("Bands only"))
                    selectedProgramShortName = "Bands only";
                else if (selectedProgram.Contains("Bodyweight"))
                {
                    selectedProgramShortName = "Bodyweight";
                    level = 1;
                }
                else if (selectedProgram.Equals("Bodyweight level 2"))
                {
                    selectedProgramShortName = "Bodyweight";
                    level = 2;
                }
                else if (selectedProgram.Equals("Bodyweight level 3"))
                {
                    selectedProgramShortName = "Bodyweight";
                    level = 3;
                }

                var mo = DrMuscle.Constants.AppThemeConstants.GetLevelProgram(level, programName.ToLower().Contains("gym"), selectedProgram.Contains("Full-body"), selectedProgramShortName);
                ConfirmConfig supersetConfig = new ConfirmConfig()
                {
                    Title = "Are you sure?",
                    Message = $"Your program will change to {mo.programName}",
                    OkText = "Change program",
                    AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                    CancelText = AppResources.Cancel,
                };

                var x = await UserDialogs.Instance.ConfirmAsync(supersetConfig);
                if (x)
                {
                    LocalDBManager.Instance.SetDBSetting("recommendedWorkoutId", mo.workoutid.ToString());
                    LocalDBManager.Instance.SetDBSetting("recommendedWorkoutLabel", mo.workoutName);
                    LocalDBManager.Instance.SetDBSetting("recommendedProgramId", mo.programid.ToString());
                    LocalDBManager.Instance.SetDBSetting("recommendedRemainingWorkout", mo.reqWorkout.ToString());

                    LocalDBManager.Instance.SetDBSetting("recommendedProgramLabel", mo.programName);
                    isAILoaded = false;
                    aiTitle = "";
                    aiDescription = "";
                    CurrentLog.Instance.AiDescription = "";
                     AnaliesAIWithChatGPT();
                    //MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage() { IsRefresh = true }, "SignupFinishMessage");
                    await DrMuscleRestClient.Instance.SaveWorkoutV3(new SaveWorkoutModel() { WorkoutId = mo.workoutid });
                    //Close_Tapped(sender, e);


                }
            }
            catch (Exception ex)
            {

            }
        }

        async Task BoomSuccessPopup()
        {
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var modalPage = new Views.GeneralPopup("Lists.png", "Settings saved", "Do a demo workout to unlock the full experience", "Demo workout", new Thickness(18, 0, 0, 0));
            modalPage.Disappearing += (sender2, e2) =>
            {
                waitHandle.Set();
            };
            await PopupNavigation.Instance.PushAsync(modalPage);

            await Task.Run(() => waitHandle.WaitOne());

        }
        async Task<DrMuscleButton> AddOptions(string title, EventHandler handler)
        {
            var grid = new Grid();
            var pancakeView = new PancakeView() { HeightRequest = 55, Margin = new Thickness(10, 2) };
            pancakeView.OffsetAngle = Device.RuntimePlatform.Equals(Device.Android) ? 45 : 90;
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#195276"), Offset = 0 });
            pancakeView.BackgroundGradientStops.Add(new Xamarin.Forms.PancakeView.GradientStop { Color = Color.FromHex("#0C2432"), Offset = 1 });
            grid.Children.Add(pancakeView);


            var btn = new DrMuscleButton()
            {
                Text = title,
                TextColor = Color.Black,
                BackgroundColor = Color.White,
                FontSize = Device.RuntimePlatform.Equals(Device.Android) ? 15 : 17,
                CornerRadius = 0,
                HeightRequest = 55
            };
            btn.Clicked += handler;
            SetDefaultButtonStyle(btn);
            grid.Children.Add(btn);
            Device.BeginInvokeOnMainThread(() =>
            {
                workoutStack.Children.Add(grid);
            });



            return btn;
        }

        void SetDefaultButtonStyle(Button btn)
        {
            btn.BackgroundColor = Color.Transparent;
            btn.BorderWidth = 0;
            btn.CornerRadius = 0;
            btn.Margin = new Thickness(25, 2, 25, 2);
            btn.FontAttributes = FontAttributes.Bold;
            btn.BorderColor = Color.Transparent;
            btn.TextColor = Color.White;
            btn.HeightRequest = 55;

        }
    }
}
