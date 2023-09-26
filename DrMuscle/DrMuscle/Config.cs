using System;
using Plugin.Settings;
using DrMuscle.Constants;
using Xamarin.Forms;

namespace DrMuscle
{
    public class Config
    {
        //

        
            public static string RegisteredDeviceToken
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("RegisteredDeviceToken", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("RegisteredDeviceToken", value);
            }
        }
        public static int SecondOpenEventTrack
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("SecondOpenEventTrack", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("SecondOpenEventTrack", value);
            }
        }

        public static bool ShowWelcomePopUp2
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWelcomePopUp2", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWelcomePopUp2", value);
            }
        }
        public static bool ShowExercisePopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowExercisePopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowExercisePopup", value);
            }
        }

        public static bool ShowGymPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowGymPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowGymPopup", value);
            }
        }

        public static bool ShowHomeGymPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowHomeGymPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowHomeGymPopup", value);
            }
        }
        public static bool ShowBodyweightPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowBodyweightPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowBodyweightPopup", value);
            }
        }

        public static bool ShowCustomPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowCustomPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowCustomPopup", value);
            }
        }

        public static bool ShowLearnPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowLearnPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowLearnPopup", value);
            }
        }

        public static bool ShowChatPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowChatPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowChatPopup", value);
            }
        }

        public static bool ShowSettingsPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowSettingsPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowSettingsPopup", value);
            }
        }

        public static bool ShowBackoffPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowBackoffPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowBackoffPopup", value);
            }
        }

        public static bool AddExercisesPopUp
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("AddExercisesPopUp", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("AddExercisesPopUp", value);
            }
        }

        public static bool IsOneRMApplied
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsOneRMApplied", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsOneRMApplied", value);
            }
        }

        public static bool ShowWarmups
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWarmups", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWarmups", value);
            }
        }

        public static bool RepRangeOutsidePopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("RepRangeOutsidePopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("RepRangeOutsidePopup", value);
            }
        }

        public static bool ShowWelcomePopUp3
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWelcomePopUp3", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWelcomePopUp3", value);
            }
        }

        public static bool ShowWelcomePopUp4
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWelcomePopUp4", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWelcomePopUp4", value);
            }
        }

        public static bool ShowAllSetPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowAllSetPopup", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowAllSetPopup", value);
            }
        }

        public static bool MobilityWelcomePopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("MobilityWelcomePopup", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("MobilityWelcomePopup", value);
            }
        }

        public static bool ShowWelcomePopUp5
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWelcomePopUp5", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWelcomePopUp5", value);
            }
        }

        public static bool ShowEasyExercisePopUp
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowEasyExercisePopUp", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowEasyExercisePopUp", value);
            }
        }

        public static bool ShowRIRPopUp
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowRIRPopUp", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowRIRPopUp", value);
            }
        }
        //
        public static bool Superset_warning_shown
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("Superset_warning_shown", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("Superset_warning_shown", value);
            }
        }

        public static int ShowPlateCalculatorPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowPlateCalculatorPopup", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowPlateCalculatorPopup", value);
            }
        }

        public static int ShowSupersetPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowSupersetPopup", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowSupersetPopup", value);
            }
        }
        //
        //public static decimal DownRecordPercentage
        //{
        //    get
        //    {
        //        return CrossSettings.Current.GetValueOrDefault("DownRecordPercentage", 0);
        //    }

        //    set
        //    {
        //        CrossSettings.Current.AddOrUpdateValue("DownRecordPercentage", value);
        //    }
        //}

        //public static string DownRecordExplainer
        //{
        //    get
        //    {
        //        return CrossSettings.Current.GetValueOrDefault("DownRecordExplainer", "");
        //    }

        //    set
        //    {
        //        CrossSettings.Current.AddOrUpdateValue("DownRecordExplainer", value);
        //    }
        //}
        public static bool ShowExplainRIRPopUp
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowExplainRIRPopUp", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowExplainRIRPopUp", value);
            }
        }

        public static bool ShowPlateTooltip
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowPlateTooltip", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowPlateTooltip", value);
            }
        }

        public static bool ShowChallenge
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowChallenge", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowChallenge", value);
            }
        }

        public static bool ShowDeload
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowDeload", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowDeload", value);
            }
        }

        public static bool ShowTimer
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowTimer", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowTimer", value);
            }
        }

        public static bool ShowEditWorkout
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowEditWorkout", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowEditWorkout", value);
            }
        }

        public static bool ShowDragnDrop
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowDragnDrop", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowDragnDrop", value);
            }
        }

        public static bool ShowBarSliderTooltip
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowBarSliderTooltip", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowBarSliderTooltip", value);
            }
        }

        public static bool ShowBarPlatesTooltip
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowBarPlatesTooltip", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowBarPlatesTooltip", value);
            }
        }


        public static bool RateAlreadyGiven
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("RateGiven", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("RateGiven", value);
            }
        }

        public static bool SurprisePopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("SurprisePopup", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("SurprisePopup", value);
            }
        }

        public static int ShowTipsNumber
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowTipsNumber", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowTipsNumber", value);
            }
        }

        public static int ShowTitleNumber
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowTitleNumber", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowTitleNumber", value);
            }
        }

        public static int ShowWorkoutImagesNumber
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ShowWorkoutImagesNumber", 0);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ShowWorkoutImagesNumber", value);
            }
        }

        public static bool ViewWebHistoryPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("ViewWebHistoryPopup", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("ViewWebHistoryPopup", value);
            }
        }

        public static DateTime LastOpenCongratsPopupDate
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("LastOpenCongratsPopupDate", DateTime.Now.Date);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("LastOpenCongratsPopupDate", value);
            }
        }

        public static bool FirstChallenge
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("FirstChallenge", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("FirstChallenge", value);
            }
        }

        public static bool FirstDeload
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("FirstDeload", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("FirstDeload", value);
            }
        }

        public static bool FirstLightSession
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("FirstLightSession", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("FirstLightSession", value);
            }
        }

        public static bool IsSwappedFirstTime
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsSwappedFirstTime", false);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsSwappedFirstTime", value);
            }
        }

        public static bool IsFirstExerciseCreatedPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsFirstExerciseCreatedPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsFirstExerciseCreatedPopup", value);
            }
        }

        public static bool IsFirstWorkoutEditedPopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsFirstWorkoutEditedPopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsFirstWorkoutEditedPopup", value);
            }
        }

        public static bool IsFirstFavoritePopup
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsFirstFavoritePopup", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsFirstFavoritePopup", value);
            }
        }

        public static bool IsExistingSubscriber
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsExistingSubscriber", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsExistingSubscriber", value);
            }
        }

        public static string UserEmail
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("UserEmail", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("UserEmail", value);
            }
        }

        public static bool IsAppUsed
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsAppUsed", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsAppUsed", value);
            }
        }

        public static bool IsMealEntered
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsMealEntered", true);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsMealEntered", value);
            }
        }

        public static bool IsMeal1
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsMeal1", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsMeal1", value);
            }
        }

        public static bool IsMeal2
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsMeal2", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsMeal2", value);
            }
        }

        public static bool IsMeal3
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("IsMeal3", false);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("IsMeal3", value);
            }
        }

        public static string UserId
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("UserId", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("UserId", value);
            }
        }

        public static string CurrentWeight
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("CurrentWeight", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("CurrentWeight", value);
            }
        }

        public static string MassUnit
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("MassUnit", "lb");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("MassUnit", value);
            }
        }

        public static string MonthAgoWeight
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("MonthAgoWeight", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("MonthAgoWeight", value);
            }
        }

        public static string PredictedWeight
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("PredictedWeight", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("PredictedWeight", value);
            }
        }

        public static string LastEatMeal
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("LastEatMeal", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("LastEatMeal", value);
            }
        }

        public static decimal NextMealHours
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("NextMealHours", 0);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("NextMealHours", value);
            }
        }

       

        public static DateTime AccountCreationDate
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("AccountCreationDate", DateTime.Now.Date);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("AccountCreationDate", value);
            }
        }

        public static string LastMealPlanOrderDate
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("LastMealPlanOrderDate", null);
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("LastMealPlanOrderDate", value);
            }
        }

        public static long LastBodyWeightUpdate
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("LastBodyWeightUpdate", default(long));
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("LastBodyWeightUpdate", value);
            }
        }

        public static string Firstname
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("Firstname", "");
            }

            set
            {
                CrossSettings.Current.AddOrUpdateValue("Firstname", value);
            }
        }

        public static float UserHeight
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("UserHeight", (float)0);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("UserHeight", value);
            }
        }

        public static int UserAge
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("UserAge", 0);
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("UserAge", value);
            }
        }

        public static string UserGender
        {
            get
            {
                return CrossSettings.Current.GetValueOrDefault("UserGender", "");
            }
            set
            {
                CrossSettings.Current.AddOrUpdateValue("UserGender", value);
            }
        }
    }
}
