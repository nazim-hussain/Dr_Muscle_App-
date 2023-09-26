using System;
using Xamarin.Forms;
using DrMuscle.Resx;
using System.Collections.Generic;
using Xamarin.Essentials;
using System.Globalization;
using DrMuscle.Helpers;

namespace DrMuscle.Constants
{
    public static class AppThemeConstants
    {
        public static String GPTKey = ""; //Enter right key
        public static string ChatBotId = "ewer325ewr324";
        public static string ChatBotSecretKey = "dsfss342fsdf3453fsdf43";
        public static Color DefaultColor = Color.White;
        public static Color LearnMoreTextColor = Color.Aqua;
        public static Color TextColorBlack = Color.FromHex("#26262B");//off Black
        public static Color LightGrayColor = Color.LightGray;
        public static Color RedColor = Color.Red;
        public static Color OutgoingBubbleColor = Color.FromHex("#555555");
        public static Color IncomingChatTextColor = Color.FromHex("#26262B");
        public static Color OutgoingChatTextColor = Color.FromHex("#26262B");
        public static Color CellItemsButtonColor = Color.FromHex("#faf0e6");
        public static Color IncomingBubbleColor = Color.FromHex("#333333");
        public static Color BlueColor = Color.FromHex("#195377");
        
        public static Color BlueStartGradient = Color.FromHex("#99195276");
        public static Color GreenGradient = Color.LightGray;//Color.FromHex("#5DD397");
        public static Color DimBlueColor = Color.FromHex("#AA195377");
        public static Color BlueLightColor = Color.FromHex("#195377");
        public static Color ReysBlueColor = Color.FromHex("#97D2F3");
        public static Color OffBlackColor = Color.FromHex("#26262B");
        public static Color DarkRedColor = Color.FromHex("#BA1C31");
        public static Color GreenColor = Color.FromHex("#5CD196");
        public static Color GreenTransparentColor = Color.FromHex("#4D5CD196");
        public static string ChatReceiverId = "bcee85f1-b0a8-4281-aca5-0af3100d5541";
        public static string ExerciseVideoLink = "https://www.bodybuilding.com/exercises/search?query=Abdominals";
        public static Dictionary<string, Color> ProfileColor = new Dictionary<string, Color>();
        private static Random randonGen = new Random();
        public static Color RandomColor => Color.FromRgb(randonGen.Next(255), randonGen.Next(255), randonGen.Next(255));
        public static double NavigationBarHeight = App.NavigationBarHeight;


        static double hgt = DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Height;
        static double wid = DeviceDisplay.MainDisplayInfo.Density > 1 ? DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density : DeviceDisplay.MainDisplayInfo.Width;
        public static double SetCellEntryHeight = hgt > 580 ? 116 : 90;
        public static float SetCellEntryCornerRadius = hgt > 580 ? 58 : 45;
        public static double SetCellEntryBigFontSize = hgt > 580 ? 29 : 24;
        public static double SetCellEntrySmallFontSize = hgt > 580 ? 14 : 10;

        public static double CapitalTitleFontSize = wid > 375 ? 14 : 13;
        public static double DescriptionFontSize = wid > 375 ? 16 : 15;
        public static double TitleFontSize = wid > 375 ? 18 : 16;
        public static decimal Coeficent = (decimal)0.0526315;//0.03921568;
        //public static Color 
        //TextColor="#5063EE" BackgroundColor="White"
        public static class SO30180672
        {
            internal static string FormatNumber(long num)
            {
                num = MaxThreeSignificantDigits(num);

                //if (num >= 100000000)
                //    return (num / 1000000D).ToString("0.#M");
                //if (num >= 1000000)
                //    return (num / 1000000D).ToString("0.##M");
                //if (num >= 100000)
                //    return (num / 1000D).ToString("0k");
                //if (num >= 100000)
                //    return (num / 1000D).ToString("0.#k");
                //if (num >= 1000)
                //    return (num / 1000D).ToString("0.##k");
                if (num > 999999999 || num < -999999999)
                {
                    return num.ToString("0,,,.### B", CultureInfo.InvariantCulture);
                }
                else
    if (num > 999999 || num < -999999)
                {
                    return num.ToString("0,,.## M", CultureInfo.InvariantCulture);
                }
                else
    if (num > 999 || num < -999)
                {
                    return num.ToString("0,.# K", CultureInfo.InvariantCulture);
                }
                else
                {
                    return num.ToString(CultureInfo.InvariantCulture);
                }

                //return num.ToString("#,0");

                return num.ToString("#,0");
            }


            internal static long MaxThreeSignificantDigits(long x)
            {
                int i = (int)Math.Log10(x);
                i = Math.Max(0, i - 2);
                i = (int)Math.Pow(10, i);
                return x / i * i;
            }
        }
        public static string TimeAgo(DateTime dateTime)
        {
            string result = string.Empty;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = AppResources.TodayLowercase;
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = AppResources.TodayLowercase;
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = AppResources.TodayLowercase;
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("{0} {1}", timeSpan.Days, AppResources.DaysAgo) :
                    $"1 {AppResources.DayAgo}";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("{0}mo ago", timeSpan.Days / 30) :
                    AppResources.AMonthAgo;
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    String.Format("{0}y ago", timeSpan.Days / 365) :
                    AppResources.AYearAgo;
            }

            return result;
        }
        public static bool IsAdminId(this string id)
        {
            return id.Equals("bcee85f1-b0a8-4281-aca5-0af3100d5541");
        }
        public static string GetBodyPartName(this long? bodypartId)
        {
            switch (bodypartId)
            {
                case 1:
                    return "Undefined";
                case 2:
                    return "Shoulders";

                case 3:
                    return "Chest";
                case 4:
                    return "Back";

                case 5:
                    return "Biceps";

                case 6:
                    return "Triceps";

                case 7:
                    return "Abs";

                case 8:
                    return "Legs";
                case 9:
                    return "Calves";
                case 10:
                    return "Neck";
                case 11:
                    return "Forearm";
                case 12:
                    return "Cardio";
                case 13:
                    return "Flexibility & Mobility";
                case 14:
                    return "Lower back, glutes & hamstrings";
                case 25:
                    return "Favorites";
                case 26:
                    return "My exercises";
                case 27:
                    return "Selected";
                case 28:
                    return "Flexibility & Mobility";
                default:
                    break;
            }
            return "Undefined";
        }
        public static string ChatTimeAgoFromDate(this DateTime createdAt)
        {
            //var time = TimeSpan.FromMilliseconds(createdAt);
            //DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var dateTime = createdAt.ToLocalTime();

            //var dateTime = DateTime.FromBinary(createdAt) + time;
            //DateTime dateTime = new DateTime(1970, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local) + time;
            string result = string.Empty;

            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("{0}d ago", timeSpan.Days) :
                    "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("{0}m ago", timeSpan.Days / 30) :
                    "a month ago";
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    String.Format("{0}y ago", timeSpan.Days / 365) :
                    "a year ago";
            }

            return result;
        }
        public static string ChatTimeAgo(this long createdAt)
        {
            var time = TimeSpan.FromMilliseconds(createdAt);
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0,0,  DateTimeKind.Utc);
            var dateTime = origin.AddMilliseconds(createdAt).ToLocalTime();

            //var dateTime = DateTime.FromBinary(createdAt) + time;
            //DateTime dateTime = new DateTime(1970, 1, 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTimeKind.Local) + time;
            string result = string.Empty;

            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = string.Format("{0}", dateTime.ToString("hh:mm tt"));
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("{0}d ago", timeSpan.Days) :
                    "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("{0}m ago", timeSpan.Days / 30) :
                    "a month ago";
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    String.Format("{0}y ago", timeSpan.Days / 365) :
                    "a year ago";
            }

            return result;
        }
        public static TempProgramModel GetLevelProgram(int level, bool isgym, bool isfullbody, string program)
        {
            var programid = 0;
            var programName = "";
            var workoutid = 0;
            var workoutName = "";
            var reqWorkout = 0;




            if (isgym)
            {
                if (isfullbody)
                {
                    switch (level)
                    {
                        case 1:
                            programid = 10;
                            programName = "[Gym] Full-Body Level 1";
                            workoutid = 104;
                            workoutName = "[Gym] Full-Body";
                            reqWorkout = 18;
                            break;
                        case 2:
                            programid = 14;
                            programName = "[Gym] Full-Body Level 2";
                            workoutid = 418;
                            workoutName = "[Gym] Full-Body 2A";
                            reqWorkout = 24;
                            break;
                        case 3:
                            programid = 43;
                            programName = "[Gym] Full-Body Level 3";
                            workoutid = 871;
                            workoutName = "[Gym] Full-Body 3A";
                            reqWorkout = 24;
                            break;
                        case 4:
                            programid = 393;
                            programName = "[Gym] Full-Body Level 4";
                            workoutid = 2294;
                            workoutName = "[Gym] Full-Body 4A (easy)";
                            reqWorkout = 24;
                            break;
                        case 5:
                            programid = 394;
                            programName = "[Gym] Full-Body Level 5";
                            workoutid = 2308;
                            workoutName = "[Gym] Full-Body 5A (easy)";
                            reqWorkout = 24;
                            break;
                        case 6:
                            programid = 395;
                            programName = "[Gym] Full-Body Level 6";
                            workoutid = 2312;
                            workoutName = "[Gym] Full-Body 6A (easy)";
                            reqWorkout = 24;
                            break;
                        case 7:
                            programid = 942;
                            programName = "[Gym] Full-Body Level 7";
                            workoutid = 14089;
                            workoutName = "[Gym] Full-Body 7A (medium)";
                            reqWorkout = 48;
                            break;
                        default:
                            programid = 10;
                            programName = "[Gym] Full-Body Level 1";
                            workoutid = 104;
                            workoutName = "[Gym] Full-Body";
                            reqWorkout = 18;
                            break;
                    }
                }
                else
                {
                    //split
                    switch (level)
                    {
                        case 1:
                            programid = 15;
                            programName = "[Gym] Up/Low Split Level 1";
                            workoutid = 106;
                            workoutName = "[Gym] Lower Body";
                            reqWorkout = 32;
                            break;
                        case 2:
                            programid = 16;
                            programName = "[Gym] Up/Low Split Level 2";
                            workoutid = 424;
                            workoutName = "[Gym] Lower Body 2A";
                            reqWorkout = 40;
                            break;
                        case 3:
                            programid = 45;
                            programName = "[Gym] Up/Low Split Level 3";
                            workoutid = 877;
                            workoutName = "[Gym] Lower Body 3A";
                            reqWorkout = 40;
                            break;
                        case 4:
                            programid = 399;
                            programName = "[Gym] Up/Low Split Level 4";
                            workoutid = 2280;
                            workoutName = "[Gym] Lower Body 4A (easy)";
                            reqWorkout = 48;
                            break;
                        case 5:
                            programid = 400;
                            programName = "[Gym] Up/Low Split Level 5";
                            workoutid = 2333;
                            workoutName = "[Gym] Lower Body 5A (easy)";
                            reqWorkout = 48;
                            break;
                        case 6:
                            programid = 401;
                            programName = "[Gym] Up/Low Split Level 6";
                            workoutid = 2337;
                            workoutName = "[Gym] Lower Body 6A (easy)";
                            reqWorkout = 48;
                            break;
                        case 7:
                            programid = 1014;
                            programName = "[Gym] Up/Low Split Level 7";
                            workoutid = 14336;
                            workoutName = "[Gym] Lower Body 7A (medium)";
                            reqWorkout = 64;
                            break;
                        default:
                            programid = 15;
                            programName = "[Gym] Up/Low Split Level 1";
                            workoutid = 106;
                            workoutName = "[Gym] Lower Body";
                            reqWorkout = 32;
                            break;
                    }
                }
                if (program.Contains("PPL") && level <= 1)
                {
                    programid = 2056;
                    programName = "[Gym] Push/Pull/Legs Level 1";
                    workoutid = 17449;
                    workoutName = "[Gym] Push";
                    reqWorkout = 48;
                }
                else if (program.Contains("PPL"))
                {
                    programid = 2099;
                    programName = "[Gym] Push/Pull/Legs Level 2";
                    workoutid = 17638;
                    workoutName = "[Gym] Push 2A";
                    reqWorkout = 60;
                }
            }
            else
            {
                //home
                if (isfullbody)
                {
                    switch (level)
                    {
                        case 1:
                            programid = 17;
                            programName = "[Home] Full-Body Level 1";
                            workoutid = 108;
                            workoutName = "[Home] Full-Body";
                            reqWorkout = 18;
                            break;
                        case 2:
                            programid = 20;
                            programName = "[Home] Full-Body Level 2";
                            workoutid = 421;
                            workoutName = "[Home] Full-Body 2A";
                            reqWorkout = 24;
                            break;
                        case 3:
                            programid = 44;
                            programName = "[Home] Full-Body Level 3";
                            workoutid = 874;
                            workoutName = "[Home] Full-Body 3A";
                            reqWorkout = 24;
                            break;
                        case 4:
                            programid = 396;
                            programName = "[Home] Full-Body Level 4";
                            workoutid = 2297;
                            workoutName = "[Home] Full-Body 4A (easy)";
                            reqWorkout = 24;
                            break;
                        case 5:
                            programid = 397;
                            programName = "[Home] Full-Body Level 5";
                            workoutid = 2320;
                            workoutName = "[Home] Full-Body 5A (easy)";
                            reqWorkout = 24;
                            break;
                        case 6:
                            programid = 398;
                            programName = "[Home] Full-Body Level 6";
                            workoutid = 2325;
                            workoutName = "[Home] Full-Body 6A (easy)";

                            reqWorkout = 24;
                            break;
                        case 7:
                            programid = 957;
                            programName = "[Home] Full-Body Level 7";
                            workoutid = 14155;
                            workoutName = "[Home] Full-Body 7A (medium)";
                            reqWorkout = 48;
                            break;
                        default:
                            programid = 17;
                            programName = "[Home] Full-Body Level 1";
                            workoutid = 108;
                            workoutName = "[Home] Full-Body";
                            reqWorkout = 18;
                            break;
                    }
                }
                else
                {
                    //split
                    switch (level)
                    {
                        case 1:
                            programid = 21;
                            programName = "[Home] Up/Low Split Level 1";
                            workoutid = 109;
                            workoutName = "[Home] Lower Body";
                            reqWorkout = 32;
                            break;
                        case 2:
                            programid = 22;
                            programName = "[Home] Up/Low Split Level 2";
                            workoutid = 428;
                            workoutName = "[Home] Lower Body 2A";
                            reqWorkout = 40;
                            break;
                        case 3:
                            programid = 46;
                            programName = "[Home] Up/Low Split Level 3";
                            workoutid = 883;
                            workoutName = "[Home] Lower Body 3A";
                            reqWorkout = 40;
                            break;
                        case 4:
                            programid = 402;
                            programName = "[Home] Up/Low Split Level 4";
                            workoutid = 2288;
                            workoutName = "[Home] Lower Body 4A (easy)";
                            reqWorkout = 48;
                            break;
                        case 5:
                            programid = 403;
                            programName = "[Home] Up/Low Split Level 5";
                            workoutid = 2357;
                            workoutName = "[Home] Lower Body 5A (easy)";
                            reqWorkout = 48;
                            break;
                        case 6:
                            programid = 404;
                            programName = "[Home] Up/Low Split Level 6";
                            workoutid = 2361;
                            workoutName = "[Home] Lower Body 6A (easy)";
                            reqWorkout = 48;
                            break;
                        case 7:
                            programid = 1015;
                            programName = "[Home] Up/Low Split Level 7";
                            workoutid = 14354;
                            workoutName = "[Home] Lower Body 7A (medium)";
                            reqWorkout = 64;
                            break;
                        default:
                            programid = 21;
                            programName = "[Home] Up/Low Split Level 1";
                            workoutid = 109;
                            workoutName = "[Home] Lower Body";
                            reqWorkout = 32;
                            break;
                    }
                }
                if (program.Contains("PPL") && level <= 1)
                {
                    programid = 2057;
                    programName = "[Home] Push/Pull/Legs Level 1";
                    workoutid = 17452;
                    workoutName = "[Home] Push";
                    reqWorkout = 48;
                }
                else if(program.Contains("PPL"))
                {
                    programid = 2100;
                    programName = "[Home] Push/Pull/Legs Level 2";
                    workoutid = 17644;
                    workoutName = "[Home] Push 2A";
                    reqWorkout = 60;
                }
            }

            if (program.Contains("Powerlifting"))
            {
                switch (level)
                {
                    case 1:
                        programid = 1731;
                        programName = "[Gym] Powerlifting Level 1";
                        workoutid = 16336;
                        workoutName = "[Gym] PL 1 - Monday";
                        reqWorkout = 18;
                        break;
                    case 2:
                        programid = 1736;
                        programName = "[Gym] Powerlifting Level 2";
                        workoutid = 16349;
                        workoutName = "[Gym] PL 2 - Monday";
                        reqWorkout = 18;
                        break;
                    case 3:
                        programid = 1735;
                        programName = "[Gym] Powerlifting Level 3";
                        workoutid = 16346;
                        workoutName = "[Gym] PL 3 - Monday";
                        reqWorkout = 18;
                        break;
                    case 4:
                        programid = 1734;
                        programName = "[Gym] Powerlifting Level 4";
                        workoutid = 16343;
                        workoutName = "[Gym] PL 4 - Monday";
                        reqWorkout = 30;
                        break;
                    default:
                        programid = 1731;
                        programName = "[Gym] Powerlifting Level 1";
                        workoutid = 16336;
                        workoutName = "[Gym] PL 1 - Monday";
                        reqWorkout = 18;
                        break;
                }
            }
            else if (program.Contains("Bodyweight"))
            {
                
                switch (level)
                {
                    case 1:
                        programid = 487;
                        programName = "Bodyweight Level 1";
                        workoutid = 12645;
                        workoutName = "Bodyweight 1";
                        reqWorkout = 12;
                        break;
                    case 2:
                        programid = 488;
                        programName = "Bodyweight Level 2";
                        workoutid = 12646;
                        workoutName = "Bodyweight 2";
                        reqWorkout = 12;
                        break;
                    case 3:
                        programid = 923;
                        programName = "Bodyweight Level 3";
                        workoutid = 14017;
                        workoutName = "Bodyweight 3";
                        reqWorkout = 15;
                        break;
                    case 4:
                        programid = 924;
                        programName = "Bodyweight Level 4";
                        workoutid = 14019;
                        workoutName = "Bodyweight 4";
                        reqWorkout = 15;
                        break;
                    default:
                        programid = 487;
                        programName = "Bodyweight Level 1";
                        workoutid = 12645;
                        workoutName = "Bodyweight 1";
                        reqWorkout = 12;
                        break;
                }
            }
            else if (program.Contains("Bands"))
            {
                switch (level)
                {
                    case 1:
                        programid = 1339;
                        programName = "Home] Buffed w/ Bands Level 1";
                        workoutid = 15377;
                        workoutName = "[Home] Buffed w/ Bands";
                        reqWorkout = 15;
                        break;
                    case 2:
                        programid = 1338;
                        programName = "[Home] Buffed w/ Bands Level 2";
                        workoutid = 15375;
                        workoutName = "[Home] Buffed w/ Bands 2A";
                        reqWorkout = 18;
                        break;
                    case 3:
                        workoutName = "[Home] Buffed w/ Bands 3A";
                        programName = "[Home] Buffed w/ Bands Level 3";
                        workoutid = 17321;
                        programid = 2032;
                        reqWorkout = 24;
                        break;
                    default:
                        programid = 1339;
                        programName = "Home] Buffed w/ Bands Level 1";
                        workoutid = 15377;
                        workoutName = "[Home] Buffed w/ Bands";
                        reqWorkout = 15;
                        break;
                }
            }
                return new TempProgramModel()
            {
                programid = programid,
                programName = programName,
                reqWorkout = reqWorkout,
                workoutid = workoutid,
                workoutName = workoutName
            };


        }

        public static TempProgramModel GetLevelLastProgram(int level, bool isgym, bool isfullbody, string program)
        {
            var programid = 0;
            var programName = "";
            var workoutid = 0;
            var workoutName = "";
            var reqWorkout = 0;
            if (isgym)
            {
                
                if (isfullbody)
                {
                    switch (level)
                    {
                        case 1:
                            programid = 10;
                            programName = "[Gym] Full-Body Level 1";
                            workoutid = 104;
                            workoutName = "[Gym] Full-Body";
                            reqWorkout = 18;
                            break;
                        case 2:
                            programid = 14;
                            programName = "[Gym] Full-Body Level 2";
                            workoutid = 419;
                            workoutName = "[Gym] Full-Body 2A";
                            reqWorkout = 24;
                            break;
                        case 3:
                            programid = 43;
                            programName = "[Gym] Full-Body Level 3";
                            workoutid = 873;
                            workoutName = "[Gym] Full-Body 3A";
                            reqWorkout = 24;
                            break;
                        case 4:
                            programid = 393;
                            programName = "[Gym] Full-Body Level 4";
                            workoutid = 2304;
                            workoutName = "[Gym] Full-Body 4A (easy)";
                            reqWorkout = 24;
                            break;
                        case 5:
                            programid = 394;
                            programName = "[Gym] Full-Body Level 5";
                            workoutid = 2311;
                            workoutName = "[Gym] Full-Body 5A (easy)";
                            reqWorkout = 24;
                            break;
                        case 6:
                            programid = 395;
                            programName = "[Gym] Full-Body Level 6";
                            workoutid = 2316;
                            workoutName = "[Gym] Full-Body 6A (easy)";
                            reqWorkout = 24;
                            break;
                        case 7:
                            programid = 942;
                            programName = "[Gym] Full-Body Level 7";
                            workoutid = 14097;
                            workoutName = "[Gym] Full-Body 7A (medium)";
                            reqWorkout = 48;
                            break;
                        default:
                            programid = 10;
                            programName = "[Gym] Full-Body Level 1";
                            workoutid = 104;
                            workoutName = "[Gym] Full-Body";
                            reqWorkout = 18;
                            break;
                    }
                }
                else
                {
                    //split
                    switch (level)
                    {
                        case 1:
                            programid = 15;
                            programName = "[Gym] Up/Low Split Level 1";
                            workoutid = 106;
                            workoutName = "[Gym] Lower Body";
                            reqWorkout = 32;
                            break;
                        case 2:
                            programid = 16;
                            programName = "[Gym] Up/Low Split Level 2";
                            workoutid = 425;
                            workoutName = "[Gym] Lower Body 2A";
                            reqWorkout = 40;
                            break;
                        case 3:
                            programid = 45;
                            programName = "[Gym] Up/Low Split Level 3";
                            workoutid = 882;
                            workoutName = "[Gym] Lower Body 3A";
                            reqWorkout = 40;
                            break;
                        case 4:
                            programid = 399;
                            programName = "[Gym] Up/Low Split Level 4";
                            workoutid = 2344;
                            workoutName = "[Gym] Lower Body 4A (easy)";
                            reqWorkout = 48;
                            break;
                        case 5:
                            programid = 400;
                            programName = "[Gym] Up/Low Split Level 5";
                            workoutid = 2347;
                            workoutName = "[Gym] Lower Body 5A (easy)";
                            reqWorkout = 48;
                            break;
                        case 6:
                            programid = 401;
                            programName = "[Gym] Up/Low Split Level 6";
                            workoutid = 2352;
                            workoutName = "[Gym] Lower Body 6A (easy)";
                            reqWorkout = 48;
                            break;
                        case 7:
                            programid = 1014;
                            programName = "[Gym] Up/Low Split Level 7";
                            workoutid = 14353;
                            workoutName = "[Gym] Lower Body 7A (medium)";
                            reqWorkout = 64;
                            break;
                        default:
                            programid = 15;
                            programName = "[Gym] Up/Low Split Level 1";
                            workoutid = 106;
                            workoutName = "[Gym] Lower Body";
                            reqWorkout = 32;
                            break;
                    }
                }
                if (program.Contains("PPL"))
                {
                    programid = 2056;
                    programName = "[Gym] Push/Pull/Legs Level 1";
                    workoutid = 17451;
                    workoutName = "[Gym] Push";
                    reqWorkout = 48;
                }
            }
            else
            {
                //home
                if (isfullbody)
                {
                    switch (level)
                    {
                        case 1:
                            programid = 17;
                            programName = "[Home] Full-Body Level 1";
                            workoutid = 108;
                            workoutName = "[Home] Full-Body";
                            reqWorkout = 18;
                            break;
                        case 2:
                            programid = 20;
                            programName = "[Home] Full-Body Level 2";
                            workoutid = 421;
                            workoutName = "[Home] Full-Body 2A";
                            reqWorkout = 24;
                            break;
                        case 3:
                            programid = 44;
                            programName = "[Home] Full-Body Level 3";
                            workoutid = 876;
                            workoutName = "[Home] Full-Body 3A";
                            reqWorkout = 24;
                            break;
                        case 4:
                            programid = 396;
                            programName = "[Home] Full-Body Level 4";
                            workoutid = 2319;
                            workoutName = "[Home] Full-Body 4A (easy)";
                            reqWorkout = 24;
                            break;
                        case 5:
                            programid = 397;
                            programName = "[Home] Full-Body Level 5";
                            workoutid = 2324;
                            workoutName = "[Home] Full-Body 5A (easy)";
                            reqWorkout = 24;
                            break;
                        case 6:
                            programid = 398;
                            programName = "[Home] Full-Body Level 6";
                            workoutid = 2329;
                            workoutName = "[Home] Full-Body 6A (easy)";
                            reqWorkout = 24;
                            break;
                        case 7:
                            programid = 957;
                            programName = "[Home] Full-Body Level 7";
                            workoutid = 14163;
                            workoutName = "[Home] Full-Body 7A (medium)";
                            reqWorkout = 48;
                            break;
                        default:
                            programid = 17;
                            programName = "[Home] Full-Body Level 1";
                            workoutid = 108;
                            workoutName = "[Home] Full-Body";
                            reqWorkout = 18;
                            break;
                    }
                }
                else
                {
                    //split
                    switch (level)
                    {
                        case 1:
                            programid = 21;
                            programName = "[Home] Up/Low Split Level 1";
                            workoutid = 109;
                            workoutName = "[Home] Lower Body";
                            reqWorkout = 32;
                            break;
                        case 2:
                            programid = 22;
                            programName = "[Home] Up/Low Split Level 2";
                            workoutid = 429;
                            workoutName = "[Home] Lower Body 2A";
                            reqWorkout = 40;
                            break;
                        case 3:
                            programid = 46;
                            programName = "[Home] Up/Low Split Level 3";
                            workoutid = 888;
                            workoutName = "[Home] Lower Body 3A";
                            reqWorkout = 40;
                            break;
                        case 4:
                            programid = 402;
                            programName = "[Home] Up/Low Split Level 4";
                            workoutid = 2368;
                            workoutName = "[Home] Lower Body 4A (easy)";
                            reqWorkout = 48;
                            break;
                        case 5:
                            programid = 403;
                            programName = "[Home] Up/Low Split Level 5";
                            workoutid = 2372;
                            workoutName = "[Home] Lower Body 5A (easy)";
                            reqWorkout = 48;
                            break;
                        case 6:
                            programid = 404;
                            programName = "[Home] Up/Low Split Level 6";
                            workoutid = 2377;
                            workoutName = "[Home] Lower Body 6A (easy)";
                            reqWorkout = 48;
                            break;
                        case 7:
                            programid = 1015;
                            programName = "[Home] Up/Low Split Level 7";
                            workoutid = 14371;
                            workoutName = "[Home] Lower Body 7A (medium)";
                            reqWorkout = 64;
                            break;
                        default:
                            programid = 21;
                            programName = "[Home] Up/Low Split Level 1";
                            workoutid = 109;
                            workoutName = "[Home] Lower Body";
                            reqWorkout = 32;
                            break;
                    }
                }

                if (program.Contains("PPL"))
                {
                    programid = 2057;
                    programName = "[Home] Push/Pull/Legs Level 1";
                    workoutid = 17454;
                    workoutName = "[Home] Push";
                    reqWorkout = 48;
                }
            }


            if (program.Contains("Powerlifting"))
            {
                switch (level)
                {
                    case 1:
                        programid = 1731;
                        programName = "[Gym] Powerlifting Level 1";
                        workoutid = 16338;
                        workoutName = "[Gym] PL 1 - Monday";
                        reqWorkout = 18;
                        break;
                    case 2:
                        programid = 1736;
                        programName = "[Gym] Powerlifting Level 2";
                        workoutid = 16351;
                        workoutName = "[Gym] PL 2 - Monday";
                        reqWorkout = 18;
                        break;
                    case 3:
                        programid = 1735;
                        programName = "[Gym] Powerlifting Level 3";
                        workoutid = 16348;
                        workoutName = "[Gym] PL 3 - Monday";
                        reqWorkout = 18;
                        break;
                    case 4:
                        programid = 1734;
                        programName = "[Gym] Powerlifting Level 4";
                        workoutid = 16345;
                        workoutName = "[Gym] PL 4 - Monday";
                        reqWorkout = 30;
                        break;
                    default:
                        programid = 1731;
                        programName = "[Gym] Powerlifting Level 1";
                        workoutid = 16338;
                        workoutName = "[Gym] PL 1 - Monday";
                        reqWorkout = 18;
                        break;
                }
            }
            else if (program.Contains("Bodyweight"))
            {

                switch (level)
                {
                    case 1:
                        programid = 487;
                        programName = "Bodyweight Level 1";
                        workoutid = 12645;
                        workoutName = "Bodyweight 1";
                        reqWorkout = 12;
                        break;
                    case 2:
                        programid = 488;
                        programName = "Bodyweight Level 2";
                        workoutid = 12646;
                        workoutName = "Bodyweight 2";
                        reqWorkout = 12;
                        break;
                    case 3:
                        programid = 923;
                        programName = "Bodyweight Level 3";
                        workoutid = 14017;
                        workoutName = "Bodyweight 3";
                        reqWorkout = 15;
                        break;
                    case 4:
                        programid = 924;
                        programName = "Bodyweight Level 4";
                        workoutid = 14019;
                        workoutName = "Bodyweight 4";
                        reqWorkout = 15;
                        break;
                    default:
                        programid = 487;
                        programName = "Bodyweight Level 1";
                        workoutid = 12645;
                        workoutName = "Bodyweight 1";
                        reqWorkout = 12;
                        break;
                }
            }
            
            else if (program.Contains("Bands"))
            {
                switch (level)
                {
                    case 1:
                        programid = 1339;
                        programName = "Home] Buffed w/ Bands Level 1";
                        workoutid = 15377;
                        workoutName = "[Home] Buffed w/ Bands";
                        reqWorkout = 15;
                        break;
                    case 2:
                        programid = 1338;
                        programName = "[Home] Buffed w/ Bands Level 2";
                        workoutid = 15376;
                        workoutName = "[Home] Buffed w/ Bands 2A";
                        reqWorkout = 18;
                        break;

                    default:
                        programid = 1339;
                        programName = "Home] Buffed w/ Bands Level 1";
                        workoutid = 15377;
                        workoutName = "[Home] Buffed w/ Bands";
                        reqWorkout = 15;
                        break;
                }
            }
            return new TempProgramModel()
            {
                programid = programid,
                programName = programName,
                reqWorkout = reqWorkout,
                workoutid = workoutid,
                workoutName = workoutName
            };
        }
    }

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.ToCharArray()[0].ToString().ToUpper() + input.Substring(1);
            }
        }
        public static string ReplaceWithDot(this string input)
        {
            switch (input)
            {
                case null: return "";
                default: return input.Replace(",", ".");
            }
        }
    }


}
