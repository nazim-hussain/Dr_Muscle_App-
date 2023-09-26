using System;
using System.Collections.Generic;
using System.Globalization;
using Acr.UserDialogs;
using DrMuscle.Dependencies;
using DrMuscle.Helpers;
using DrMuscle.Message;
using DrMuscle.Resx;
using DrMuscle.Screens.User.OnBoarding;
using DrMuscle.Services;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using XFShapeView;

namespace DrMuscle.Views
{
    public partial class ReminderPopup : PopupPage
    {

        public bool IsSunday { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        private IAlarmAndNotificationService alarmAndNotificationService;
        public ReminderPopup()
        {
            InitializeComponent();
            timePicker.Time = new TimeSpan(09, 0, 0);
            LblProgramName.Text = "";
            alarmAndNotificationService = new AlarmAndNotificationService();
            int age = 40, xDays = -1;
            if (LocalDBManager.Instance.GetDBSetting("Age") != null && LocalDBManager.Instance.GetDBSetting("Age").Value != null)
            {
                age = int.Parse(LocalDBManager.Instance.GetDBSetting("Age").Value);
            }
            if (LocalDBManager.Instance.GetDBSetting("ReminderTime") == null || LocalDBManager.Instance.GetDBSetting("ReminderTime").Value == null)
                timePicker.Time = new TimeSpan(09, 00, 00);
            else
            {
                try
                {
                    timePicker.Time = TimeSpan.Parse(LocalDBManager.Instance.GetDBSetting("ReminderTime").Value);
                }
                catch (Exception ex)
                {

                }
            }
            //LocalDBManager.Instance.SetDBSetting("ReminderTime", uim.ReminderTime.ToString());
            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") == null || LocalDBManager.Instance.GetDBSetting("ReminderDays").Value == null)
            {
                try
                {

                    if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                            LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                    {
                        LblProgramName.Text = "For your program " + LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value;

                        if (age != -1)
                        {
                            if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                            {
                                xDays = 6;
                            }
                                else if (LblProgramName.Text.ToLower().Contains("split"))
                            {
                                if (age < 30)
                                    xDays = 4;
                                else if (age >= 30 && age <= 50)
                                    xDays = 4;
                                else
                                    xDays = 3;
                            }
                            else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                            {
                                if (age < 30)
                                    xDays = 4;
                                else if (age >= 30 && age <= 50)
                                    xDays = 3;
                                else
                                    xDays = 2;
                            }
                        }
                        if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("push/pull/legs"))
                        {
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "1111011");
                            LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "6" : xDays.ToString());
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("split"))
                        {
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0110110");
                            LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "4" : xDays.ToString());
                        }
                        else if (LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("bodyweight") || LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("full-body") || LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value.ToLower().Contains("powerlifting"))
                        {
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0101010");
                            LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "3" : xDays.ToString());
                        }
                    }
                    //}
                    //else
                    //{
                    //    if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                    //    {
                    //        LblProgramName.Text = "For your program " + workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label;
                    //        if (LblProgramName.Text.ToLower().Contains("split"))
                    //        {
                    //            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0110110");
                    //            LblInstruction.Text = "4x / week for best results";

                    //        }
                    //        else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body"))
                    //        {
                    //            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0101010");
                    //            LblInstruction.Text = "3x / week for best results";
                    //        }
                    //    }
                    //}
                    //}



                    else
                    {
                        var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                        
                        if (workouts != null)
                        {
                            if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram == null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                            {
                                if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                        LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                                {
                                    LblProgramName.Text = "For your program " + LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value;
                                    if (age != -1)
                                    {
                                        if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                        {
                                            xDays = 6;
                                        }
                                            if (LblProgramName.Text.ToLower().Contains("split"))
                                        {
                                            if (age < 30)
                                                xDays = 4;
                                            else if (age >= 30 && age <= 50)
                                                xDays = 4;
                                            else
                                                xDays = 3;
                                        }
                                        else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                        {
                                            if (age < 30)
                                                xDays = 4;
                                            else if (age >= 30 && age <= 50)
                                                xDays = 3;
                                            else
                                                xDays = 2;
                                        }
                                    }
                                    if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && !workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                        LblInstruction.Text = "3-5x / week for best results";
                                    else if (LblProgramName.Text.ToLower().Contains("split"))
                                        LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "4" : xDays.ToString());
                                    else if (LblProgramName.Text.ToLower().Contains("ppl"))
                                        LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "6" : xDays.ToString());
                                    else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                        LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "3" : xDays.ToString());
                                    else
                                        LblInstruction.Text = "";
                                }
                            }
                            else
                            {
                                if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                                {
                                    LblProgramName.Text = "For your program " + workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label;
                                    if (age != -1)
                                    {
                                        if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                        {
                                        }
                                            else if (LblProgramName.Text.ToLower().Contains("split"))
                                        {
                                            if (age < 30)
                                                xDays = 4;
                                            else if (age >= 30 && age <= 50)
                                                xDays = 4;
                                            else
                                                xDays = 3;
                                        }
                                        else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                        {
                                            if (age < 30)
                                                xDays = 4;
                                            else if (age >= 30 && age <= 50)
                                                xDays = 3;
                                            else
                                                xDays = 2;
                                        }
                                    }
                                    if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && !workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                        LblInstruction.Text = "3-5x / week for best results";
                                    else if (LblProgramName.Text.ToLower().Contains("split"))
                                        LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "4" : xDays.ToString());
                                    else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                        LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "3" : xDays.ToString());

                                    else
                                        LblInstruction.Text = "";
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LocalDBManager.Instance.SetDBSetting("ReminderDays", "0000000");
                }


            }
            else
            {
                try
                {
                    var workouts = ((App)Application.Current).UserWorkoutContexts.workouts;
                    
                    if (workouts != null)
                    {
                        if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram == null && workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate == null)
                        {
                            if (LocalDBManager.Instance.GetDBSetting("recommendedWorkoutId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedWorkoutLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramId") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel") != null &&
                                    LocalDBManager.Instance.GetDBSetting("recommendedRemainingWorkout") != null)
                            {
                                LblProgramName.Text = "For your program " + LocalDBManager.Instance.GetDBSetting("recommendedProgramLabel").Value;
                                if (age != -1)
                                {
                                    if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                    {
                                        xDays = 6;
                                    }
                                    else if (LblProgramName.Text.ToLower().Contains("split"))
                                    {
                                        if (age < 30)
                                            xDays = 4;
                                        else if (age >= 30 && age <= 50)
                                            xDays = 4;
                                        else
                                            xDays = 3;
                                    }
                                    else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                    {
                                        if (age < 30)
                                            xDays = 4;
                                        else if (age >= 30 && age <= 50)
                                            xDays = 3;
                                        else
                                            xDays = 2;
                                    }
                                }
                                if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && !workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                    LblInstruction.Text = "3-5x / week for best results";
                                else if (LblProgramName.Text.ToLower().Contains("split"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "4" : xDays.ToString());
                                else if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "6" : xDays.ToString());
                                else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "3" : xDays.ToString());
                                else
                                    LblInstruction.Text = "";
                            }
                        }
                        else
                        {
                            if (workouts.GetUserProgramInfoResponseModel.RecommendedProgram != null)
                            {
                                LblProgramName.Text = "For your program " + workouts.GetUserProgramInfoResponseModel.RecommendedProgram.Label;
                                if (age != -1)
                                {
                                    if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                    {
                                        xDays = 6;
                                    }
                                    else if (LblProgramName.Text.ToLower().Contains("split"))
                                    {
                                        if (age < 30)
                                            xDays = 5;
                                        else if (age >= 30 && age <= 50)
                                            xDays = 4;
                                        else
                                            xDays = 3;
                                    }
                                    else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                    {
                                        if (age < 30)
                                            xDays = 4;
                                        else if (age >= 30 && age <= 50)
                                            xDays = 3;
                                        else
                                            xDays = 2;
                                    }
                                }
                                if (workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate != null && !workouts.GetUserProgramInfoResponseModel.NextWorkoutTemplate.IsSystemExercise)
                                    LblInstruction.Text = "3-5x / week for best results";
                                else if (LblProgramName.Text.ToLower().Contains("split"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "4" : xDays.ToString());
                                else if (LblProgramName.Text.ToLower().Contains("push/pull/legs"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "6" : xDays.ToString());
                                else if (LblProgramName.Text.ToLower().Contains("bodyweight") || LblProgramName.Text.ToLower().Contains("full-body") || LblProgramName.Text.ToLower().Contains("bands") || LblProgramName.Text.ToLower().Contains("powerlifting"))
                                    LblInstruction.Text = string.Format("{0}x / week for best results", age == -1 && xDays == -1 ? "3" : xDays.ToString());
                                else
                                    LblInstruction.Text = "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            if (LocalDBManager.Instance.GetDBSetting("Registring")?.Value == "true")
            {
                LblProgramName.Text = "For your program " + LocalDBManager.Instance.GetDBSetting("CustomProgramName").Value;

                if (LocalDBManager.Instance.GetDBSetting("MainProgram") != null && LocalDBManager.Instance.GetDBSetting("MainProgram").Value != null)
                {

                    if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("PPL"))
                    {
                        xDays = 6;
                    }
                       else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split"))
                    {
                        if (age < 30)
                            xDays = 4;
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
                    if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("PPL"))
                    {
                       
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "1111011");

                        LblInstruction.Text = string.Format("{0}x / week for best results", xDays);
                    }
                    else if (LocalDBManager.Instance.GetDBSetting("MainProgram").Value.Contains("Split"))
                    {
                        if (xDays == 5)
                        LocalDBManager.Instance.SetDBSetting("ReminderDays", "0111110");
                        else if (xDays == 4)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0110110");
                        else if (xDays == 3)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0101010");

                        LblInstruction.Text = string.Format("{0}x / week for best results", xDays );
                    }
                    else 
                    {
                        if (xDays == 4)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0110110");
                        else if (xDays == 3)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0101010");
                        else if (xDays == 2)
                            LocalDBManager.Instance.SetDBSetting("ReminderDays", "0100100");
                        LblInstruction.Text = string.Format("{0}x / week for best results",  xDays);
                    }
                }
            }
            SetDays();
        }

        private void SetDays()
        {

            if (LocalDBManager.Instance.GetDBSetting("ReminderDays") != null && LocalDBManager.Instance.GetDBSetting("ReminderDays").Value != null)
            {
                var strDays = LocalDBManager.Instance.GetDBSetting("ReminderDays").Value;
                if (strDays.ToCharArray().Length == 7)
                {
                    IsSunday = strDays[0] == '1';
                    SetDays(SundayCircle, LblSunday, IsSunday);

                    IsMonday = strDays[1] == '1';
                    SetDays(MondayCircle, LblMonday, IsMonday);

                    IsTuesday = strDays[2] == '1';
                    SetDays(TuesdayCircle, LblTuesday, IsTuesday);

                    IsWednesday = strDays[3] == '1';
                    SetDays(WednesdayCircle, LblWednesday, IsWednesday);

                    IsThursday = strDays[4] == '1';
                    SetDays(ThursdayCircle, LblThursday, IsThursday);

                    IsFriday = strDays[5] == '1';
                    SetDays(FridayCircle, LblFriday, IsFriday);

                    IsSaturday = strDays[6] == '1';
                    SetDays(SaturdayCircle, LblSaturday, IsSaturday);
                }
            }
        }
        private void SetDays(ShapeView shapeView, Label label, bool Enabled)
        {
            if (Enabled)
            {
                shapeView.Color = Color.FromHex("#ECFF92");
                label.TextColor = Color.FromHex("#0C2432");
            }
            else
            {
                shapeView.Color = Color.Transparent;
                label.TextColor = Color.White;
            }
        }

        public void SundayTapped(object sender, EventArgs args)
        {
            IsSunday = !IsSunday;
            SetDays(SundayCircle, LblSunday, IsSunday);
        }

        public void MondayTapped(object sender, EventArgs args)
        {
            IsMonday = !IsMonday;
            SetDays(MondayCircle, LblMonday, IsMonday);
        }

        public void TuesdayTapped(object sender, EventArgs args)
        {
            IsTuesday = !IsTuesday;
            SetDays(TuesdayCircle, LblTuesday, IsTuesday);
        }

        public void WednesdayTapped(object sender, EventArgs args)
        {
            IsWednesday = !IsWednesday;
            SetDays(WednesdayCircle, LblWednesday, IsWednesday);

        }

        public void ThursdayTapped(object sender, EventArgs args)
        {
            IsThursday = !IsThursday;
            SetDays(ThursdayCircle, LblThursday, IsThursday);
        }

        public void FridayTapped(object sender, EventArgs args)
        {
            IsFriday = !IsFriday;
            SetDays(FridayCircle, LblFriday, IsFriday);
        }

        public void SaturdayTapped(object sender, EventArgs args)
        {
            IsSaturday = !IsSaturday;
            SetDays(SaturdayCircle, LblSaturday, IsSaturday);
        }

        public async void ButtonDone_Clicked(object sender, EventArgs args)
        {
            try
            {
                SaveButton.Clicked -= ButtonDone_Clicked;
                if (!DependencyService.Get<INotificationsInterface>().registeredForNotifications())
                {
                    ConfirmConfig ShowOffPopUp = new ConfirmConfig()
                    {
                        Title = "Enable notifcations",
                        Message = "Notifcations off. Enable to get reminders.",
                        AndroidStyleId = DependencyService.Get<IStyles>().GetStyleId(EAlertStyles.AlertDialogCustomGray),
                        OkText = "Enable",
                        CancelText = AppResources.Cancel,
                    };
                    var isConfirm = await UserDialogs.Instance.ConfirmAsync(ShowOffPopUp);
                    if (isConfirm)
                    {
                        DependencyService.Get<IAppSettingsHelper>().OpenAppSettings();
                    }
                }
                //Create string to save
                var ReminderDays = "";
                ReminderDays = IsSunday ? "1" : "0";
                ReminderDays += IsMonday ? "1" : "0";
                ReminderDays += IsTuesday ? "1" : "0";
                ReminderDays += IsWednesday ? "1" : "0";
                ReminderDays += IsThursday ? "1" : "0";
                ReminderDays += IsFriday ? "1" : "0";
                ReminderDays += IsSaturday ? "1" : "0";
                LocalDBManager.Instance.SetDBSetting("ReminderDays", ReminderDays);
                LocalDBManager.Instance.SetDBSetting("ReminderTime", timePicker.Time.ToString());

               
                if (LocalDBManager.Instance.GetDBSetting("email") != null)
                {
                    if (App.IsNUX)
                    {
                        DrMuscleRestClient.Instance.SetUserReminderTimeWithoutLoader(new DrMuscleWebApiSharedModel.UserInfosModel()
                        {
                            ReminderTime = timePicker.Time,
                            ReminderDays = ReminderDays
                        });
                    }
                    else
                    {

                         DrMuscleRestClient.Instance.SetUserReminderTimeWithoutLoader(new DrMuscleWebApiSharedModel.UserInfosModel()
                        {
                            ReminderTime = timePicker.Time,
                            ReminderDays = ReminderDays
                        });
                    }
                }
                LocalDBManager.Instance.SetDBSetting("RecommendedReminder", ReminderDays.Contains("1") ? "false" : "null");
                alarmAndNotificationService.CancelNotification(101);
                alarmAndNotificationService.CancelNotification(102);
                alarmAndNotificationService.CancelNotification(103);
                alarmAndNotificationService.CancelNotification(104);
                alarmAndNotificationService.CancelNotification(105);
                alarmAndNotificationService.CancelNotification(106);
                alarmAndNotificationService.CancelNotification(107);
                alarmAndNotificationService.CancelNotification(108);
                App.workoutPerDay = 0;
                System.Diagnostics.Debug.WriteLine($"Day {DateTime.Now.DayOfWeek}");
                var dayofweek = DateTime.Now.DayOfWeek;
                var day = 0;
                if (IsSunday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Sunday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Sunday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 101, NotificationInterval.Week);
                }
                if (IsMonday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Monday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Monday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Monday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 102, NotificationInterval.Week);
                }
                if (IsTuesday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Tuesday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Tuesday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Tuesday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 103, NotificationInterval.Week);
                }
                if (IsWednesday)
                {
                    App.workoutPerDay += 1;
                    if ((DayOfWeek.Wednesday - DateTime.Now.DayOfWeek) < 0)
                    {
                        day = 7 + (DayOfWeek.Wednesday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Wednesday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 104, NotificationInterval.Week);
                }
                if (IsThursday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Thursday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Thursday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Thursday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 105, NotificationInterval.Week);
                }
                if (IsFriday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Friday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Friday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Friday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 106, NotificationInterval.Week);
                }
                if (IsSaturday)
                {
                    App.workoutPerDay += 1;
                    if (DayOfWeek.Saturday - DateTime.Now.DayOfWeek < 0)
                    {
                        day = 7 + (DayOfWeek.Saturday - DateTime.Now.DayOfWeek);
                    }
                    else
                    {
                        day = DayOfWeek.Saturday - DateTime.Now.DayOfWeek;
                    }
                    var timeSpan = new TimeSpan(day, timePicker.Time.Hours, timePicker.Time.Minutes, timePicker.Time.Seconds);
                    alarmAndNotificationService.ScheduleNotification("Workout time!", "Ready to crush your workout?\nYou got this!", timeSpan, 107, NotificationInterval.Week);
                }

            }
            catch (Exception ex)
            {

            }
            //if (LocalDBManager.Instance.GetDBSetting("Registring")?.Value != "true")
            //{
            //    if (App.IsWelcomeBack)
            //    {
            //        App.IsWelcomeBack = false;
            //        CurrentLog.Instance.IsSendOne = false;
            //        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //    }
            //}
            finally
            {
            PopupNavigation.Instance.PopAsync();
                SaveButton.Clicked += ButtonDone_Clicked;
            }
        }

        public void TimerTapped(object sender, EventArgs args)
        {
            timePicker.IsVisible = true;
            timePicker.Focus();
        }

        public void ButtonSkip_Clicked(object sender, EventArgs args)
        {
            //if (LocalDBManager.Instance.GetDBSetting("Registring")?.Value != "true")
            //{
            //    if (App.IsWelcomeBack)
            //    {
            //        App.IsWelcomeBack = false;
            //        CurrentLog.Instance.IsSendOne = false;
            //        MessagingCenter.Send<SignupFinishMessage>(new SignupFinishMessage(), "SignupFinishMessage");
            //    }
            //}
            App.workoutPerDay = 0;
            PopupNavigation.Instance.PopAsync();
        }

        public void Timer_Unfocused(object sender, EventArgs args)
        {

        }


    }
}
