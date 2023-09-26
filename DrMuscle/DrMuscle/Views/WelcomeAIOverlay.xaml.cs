using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using DrMuscle.Constants;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Layout;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.Exercises;
using DrMuscle.Screens.User;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscleWebApiSharedModel;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace DrMuscle.Views
{	
	public partial class WelcomeAIOverlay : PopupPage
    {
        
        public WelcomeAIOverlay ()
		{
			InitializeComponent ();
            
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
        }

        public void SetDetails(string title, string desc)
        {
            var name = LocalDBManager.Instance.GetDBSetting("firstname")?.Value;
            LblGptTitle.Text = $"Welcome aboard {name}! It's great to have you here"; //
            if (string.IsNullOrEmpty(desc))
            {
                AnaliesAIWithChatGPT();
            } else {
                LblGptDesc.Text = $"As your AI coach, I am pumped to work with you to reach your fitness goals. Let's get started!\n\n{desc}";
                
               
                
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

        async void Close_Tapped(object sender, EventArgs e)
        {
            //try
            //{
            //    await BoomSuccessPopup();
            //    if (PopupNavigation.Instance.PopupStack.Count > 0)
            //        PopupNavigation.Instance.PopAllAsync();
            //    await OpenDemo();
            //}
            //catch (Exception ex)
            //{

            //}
            //PopupNavigation.Instance.PushAsync(new WorkoutPreviewOverlay());

            if (!CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("DemoWorkoutPage"))
            {
                if (((App)Application.Current).UserWorkoutContexts.workouts != null)
                {
                    ((App)Application.Current).UserWorkoutContexts.workouts.LastWorkoutDate = DateTime.UtcNow;
                    ((App)Application.Current).UserWorkoutContexts.SaveContexts();
                }
            }
            CurrentLog.Instance.IsFromEndExercise = false;
            
             if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisePage"))
                await PagesFactory.PushAsync<AllExercisePage>();
            else if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("AllExercisesView"))
                await PagesFactory.PushAsync<AllExercisePage>();
            else if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("KenkoSingleExercisePage"))
                await PagesFactory.PushAsync<AllExercisePage>();
            else if (CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("DemoWorkoutPage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("NewDemoPage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("NewDemoPage2") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("DemoChallengePage") || CurrentLog.Instance.EndExerciseActivityPage.FullName.Contains("RightSideMasterPage"))
            {
                CurrentLog.Instance.IsDemoRunningStep1 = true;

                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    PopupNavigation.Instance.PopAsync();
                CurrentLog.Instance.IsDemoRunningStep2 = false;
                CurrentLog.Instance.IsDemoRunningStep1 = false;
                long workoutTemplateId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId").Value);
                long programId = Convert.ToInt64(LocalDBManager.Instance.GetDBSetting("recommendedProgramId").Value);

                var upi = new GetUserProgramInfoResponseModel()
                {
                    NextWorkoutTemplate = new WorkoutTemplateModel() { Id = workoutTemplateId, Label = LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel").Value },
                    RecommendedProgram = new WorkoutTemplateGroupModel() { Id = programId, Label = LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value, RemainingToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value), RequiredWorkoutToLevelUp = Convert.ToInt32(LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout").Value) },
                };
                if (upi != null)
                {
                    CurrentLog.Instance.WorkoutStarted = true;
                    if (Device.RuntimePlatform.Equals(Device.Android))
                    {

                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        CurrentLog.Instance.Exercise1RM.Clear();

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await PagesFactory.PopToRootAsync(true);
                            await Task.Delay(1000);
                            MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                        });

                    }
                    else
                    {

                        App.IsDemoProgress = false;
                        App.IsWelcomeBack = true;
                        App.IsNewUser = true;
                        LocalDBManager.Instance.SetDBSetting("DemoProgress", "false");
                        CurrentLog.Instance.Exercise1RM.Clear();
                        await PagesFactory.PopToRootMoveAsync(true);
                        await Task.Delay(1000);
                        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
                    }


                }




            }
            else
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    PopupNavigation.Instance.PopAsync();
                CurrentLog.Instance.IsMovingOnBording = true;
                PagesFactory.PushAsync<MainOnboardingPage>();
                return;
                CurrentLog.Instance.IsFromEndExercise = true;
                PagesFactory.PopAsync();
            }
        }

        private async Task<string> AnaliesAIWithChatGPT(bool isloader = false, double temperature = 0.7, int maxTokens = 2500, double topP = 1, double frequencyPenalty = 0, double presencePenalty = 0)
        {
            return "AI text";
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
    }
}

